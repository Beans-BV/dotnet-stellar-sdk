using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     HTTP message handler that implements retry logic for connection failures (network errors, DNS failures, etc.)
///     and for configured HTTP status codes (see <see cref="HttpResilienceOptions.RetryHttpStatusCodes" />).
///     Honors the <c>Retry-After</c> response header when <see cref="HttpResilienceOptions.RespectRetryAfter" />
///     is true. By default, only safe HTTP methods (GET, HEAD, OPTIONS) are retried on status codes;
///     connection-failure (exception) retries apply to all methods.
///     <para>
///         Retries re-send the same <see cref="HttpRequestMessage" /> instance, so request bodies must be
///         re-readable, buffered content (<c>StringContent</c>, <c>ByteArrayContent</c>,
///         <c>FormUrlEncodedContent</c> — all of the SDK's own requests qualify). A non-seekable
///         <c>StreamContent</c> body fails on the second attempt.
///     </para>
///     <para>
///         Cancellations are never retried: when <c>HttpClient.Timeout</c> fires it cancels the request's
///         token, so the resulting <see cref="System.Threading.Tasks.TaskCanceledException" /> propagates
///         immediately, exactly like a user-initiated cancellation.
///     </para>
/// </summary>
public class RetryingHttpMessageHandler : DelegatingHandler
{
    private static readonly ResiliencePropertyKey<HttpRequestMessage?> RequestKey = new("StellarSdk.HttpRequest");
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RetryingHttpMessageHandler" /> class.
    /// </summary>
    /// <param name="innerHandler">The inner handler to delegate to.</param>
    /// <param name="options">
    ///     The resilience options. If null, default options are used. The instance is copied at construction;
    ///     mutating it afterwards has no effect on this handler.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when innerHandler is null.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <see cref="HttpResilienceOptions.BaseDelay" /> exceeds
    ///     <see cref="HttpResilienceOptions.MaxDelay" /> (validated here rather than in the property setters so
    ///     the two can be assigned in any order via an object initializer).
    /// </exception>
    public RetryingHttpMessageHandler(HttpMessageHandler innerHandler, HttpResilienceOptions? options = null)
        : base(innerHandler ?? throw new ArgumentNullException(nameof(innerHandler)))
    {
        // Snapshot the options. The scalar settings are baked into the pipeline below either way, but the
        // set-typed properties are read by the ShouldHandle/DelayGenerator closures on every request —
        // without a copy, post-construction mutations would half-apply (and HashSet is not thread-safe
        // under mutation while requests are in flight).
        var opts = (options ?? new HttpResilienceOptions()).Clone();
        _pipeline = BuildPipeline(opts);
    }

    /// <summary>
    ///     Sends an HTTP request through the resilience pipeline.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The HTTP response message.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Polly's ShouldHandle/DelayGenerator callbacks receive only the Outcome and ResilienceContext, not the
        // original request. Stash the request in the context's property bag so ShouldHandleOutcome can inspect the
        // HTTP method for safe/unsafe gating. The context is pooled, so it is returned in the finally block below.
        var context = ResilienceContextPool.Shared.Get(cancellationToken);
        context.Properties.Set(RequestKey, request);
        try
        {
            return await _pipeline.ExecuteAsync(
                async ctx => await base.SendAsync(request, ctx.CancellationToken).ConfigureAwait(false),
                context).ConfigureAwait(false);
        }
        finally
        {
            // Release the request reference before returning the context to the pool.
            context.Properties.Set(RequestKey, null);
            ResilienceContextPool.Shared.Return(context);
        }
    }

    /// <summary>
    ///     Sends an HTTP request through the resilience pipeline synchronously. Mirrors
    ///     <see cref="SendAsync" /> so that <see cref="HttpClient.Send(HttpRequestMessage)" /> receives the
    ///     same retry, circuit-breaker, and timeout behavior instead of silently bypassing the pipeline
    ///     (the inherited <see cref="DelegatingHandler.Send(HttpRequestMessage, CancellationToken)" />
    ///     forwards straight to the inner handler). The inner handler chain must itself support synchronous
    ///     <c>Send</c> (<see cref="SocketsHttpHandler" /> does over HTTP/1.1).
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The HTTP response message.</returns>
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = ResilienceContextPool.Shared.Get(cancellationToken);
        context.Properties.Set(RequestKey, request);
        try
        {
            return _pipeline.Execute(ctx => base.Send(request, ctx.CancellationToken), context);
        }
        finally
        {
            context.Properties.Set(RequestKey, null);
            ResilienceContextPool.Shared.Return(context);
        }
    }

    private static ResiliencePipeline<HttpResponseMessage> BuildPipeline(HttpResilienceOptions options)
    {
        var builder = new ResiliencePipelineBuilder<HttpResponseMessage>();

        // Enforce the BaseDelay <= MaxDelay invariant whenever a handler is built — even with retries
        // disabled, the pair is invalid together and accepting it would let an invalid configuration go
        // unnoticed until retries are enabled later. (Deferred from the property setters so the two can be
        // assigned in any order via an object initializer.)
        if (options.BaseDelay > options.MaxDelay)
        {
            throw new ArgumentException(
                $"BaseDelay ({options.BaseDelay.TotalMilliseconds}ms) cannot exceed " +
                $"MaxDelay ({options.MaxDelay.TotalMilliseconds}ms).",
                nameof(options));
        }

        // Timeout (outermost - applies to entire operation)
        if (options.RequestTimeout.HasValue)
        {
            builder.AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = options.RequestTimeout.Value,
            });
        }

        // Circuit Breaker (middle layer)
        if (options.EnableCircuitBreaker)
        {
            builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                FailureRatio = options.FailureRatio,
                MinimumThroughput = options.MinimumThroughput,
                SamplingDuration = options.SamplingDuration,
                BreakDuration = options.BreakDuration,
                ShouldHandle = args =>
                {
                    var cancellationRequested = args.Context.CancellationToken.IsCancellationRequested;
                    return new ValueTask<bool>(ShouldCountForBreaker(args.Outcome, options,
                        cancellationRequested));
                },
            });
        }

        // Retry (innermost - executes first)
        if (options.MaxRetryCount > 0)
        {
            var retryOptions = new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = options.MaxRetryCount,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = options.UseJitter,
                Delay = options.BaseDelay,
                MaxDelay = options.MaxDelay,
                ShouldHandle = args =>
                {
                    var cancellationRequested = args.Context.CancellationToken.IsCancellationRequested;
                    var request = args.Context.Properties.GetValue(RequestKey, null);
                    return new ValueTask<bool>(ShouldHandleOutcome(args.Outcome, request, options,
                        cancellationRequested));
                },
                DelayGenerator = args =>
                {
                    // Returning null tells Polly to fall back to its configured backoff (exponential + jitter),
                    // not to use a zero delay. We only override that backoff to honor a Retry-After header.
                    if (!options.RespectRetryAfter || options.RetryHttpStatusCodes.Count == 0)
                    {
                        return new ValueTask<TimeSpan?>((TimeSpan?)null);
                    }

                    var response = args.Outcome.Result;
                    var parsed = RetryAfterParser.ToTimeSpan(response?.Headers.RetryAfter);
                    if (parsed is null && response is { } r &&
                        r.Headers.TryGetValues("Retry-After", out var rawValues))
                    {
                        // The typed header property is null for values .NET cannot represent (delta-seconds
                        // beyond int.MaxValue, multiple values). Fall back to parsing the first raw string so
                        // the same header yields the same delay here as on the typed exceptions — still
                        // subject to the MaxRetryAfterDelay cap below.
                        foreach (var raw in rawValues)
                        {
                            parsed = RetryAfterParser.Parse(raw);
                            break;
                        }
                    }

                    if (parsed is { } delay)
                    {
                        // Polly does not apply RetryStrategyOptions.MaxDelay to a value returned from
                        // DelayGenerator. Cap the server-provided Retry-After against the dedicated
                        // MaxRetryAfterDelay ceiling (not the backoff MaxDelay), so a server asking for tens
                        // of seconds on 429 is honored instead of being truncated to the small backoff cap.
                        if (delay > options.MaxRetryAfterDelay)
                        {
                            delay = options.MaxRetryAfterDelay;
                        }

                        return new ValueTask<TimeSpan?>(delay);
                    }

                    return new ValueTask<TimeSpan?>((TimeSpan?)null);
                },
                OnRetry = args =>
                {
                    // Dispose the response we're abandoning so its content/connection isn't held until GC
                    // across retries. The final response (returned to the caller) never passes through OnRetry.
                    args.Outcome.Result?.Dispose();
                    return default;
                },
            };

            builder.AddRetry(retryOptions);
        }

        return builder.Build();
    }

    private static bool ShouldHandleOutcome(
        Outcome<HttpResponseMessage> outcome,
        HttpRequestMessage? request,
        HttpResilienceOptions options,
        bool cancellationRequested)
    {
        if (outcome.Exception != null)
        {
            return IsRetriableException(outcome.Exception, options, cancellationRequested);
        }

        if (outcome.Result is { } response &&
            options.RetryHttpStatusCodes.Contains(response.StatusCode))
        {
            // The request is observed via the Polly context property bag (see SendAsync, which always sets
            // it). Should it ever be absent — the pipeline executed some way other than SendAsync — fail
            // closed rather than risk replaying a non-idempotent method on a status code.
            return request != null && options.RetryHttpMethods.Contains(request.Method);
        }

        return false;
    }

    /// <summary>
    ///     Failure-counting predicate for the circuit breaker. Unlike the retry predicate
    ///     (<see cref="ShouldHandleOutcome" />), status-code failures are counted for every HTTP method:
    ///     the <see cref="HttpResilienceOptions.RetryHttpMethods" /> gate exists to prevent replaying
    ///     non-idempotent requests, but counting a failure replays nothing — and a breaker that ignores
    ///     write-path failures never opens during a real outage.
    /// </summary>
    private static bool ShouldCountForBreaker(
        Outcome<HttpResponseMessage> outcome,
        HttpResilienceOptions options,
        bool cancellationRequested)
    {
        if (outcome.Exception != null)
        {
            return IsRetriableException(outcome.Exception, options, cancellationRequested);
        }

        return outcome.Result is { } response &&
               options.RetryHttpStatusCodes.Contains(response.StatusCode);
    }

    private static bool IsRetriableException(Exception exception, HttpResilienceOptions options,
        bool cancellationRequested)
    {
        // A cancellation that was actually requested (user token or HttpClient.Timeout) is never retried
        // and never counted as a breaker failure — even when its exception type was registered in
        // AdditionalRetriableExceptionTypes. This gate must run before the registered-types loop so that
        // registering TaskCanceledException (or a base type) cannot turn real cancellations into failures.
        if (exception is OperationCanceledException && cancellationRequested)
        {
            return false;
        }

        // Check additional retriable exception types first
        var exceptionType = exception.GetType();
        if (options.AdditionalRetriableExceptionTypes.Count > 0)
        {
            foreach (var retriableType in options.AdditionalRetriableExceptionTypes)
            {
                if (retriableType.IsAssignableFrom(exceptionType))
                {
                    return true;
                }
            }
        }

        // HttpRequestException - network errors, always retriable
        if (exception is HttpRequestException)
        {
            return true;
        }

        // TimeoutException - always retriable
        if (exception is TimeoutException)
        {
            return true;
        }

        // TaskCanceledException without the request token signaled: thrown by a custom inner handler as a
        // timeout shape, not a cancellation (the requested-cancellation case returned false above).
        if (exception is TaskCanceledException)
        {
            return true;
        }

        return false;
    }
}