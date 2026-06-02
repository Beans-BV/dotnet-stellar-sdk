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
///     is true. By default, only safe HTTP methods (GET, HEAD, OPTIONS) are retried on status codes.
/// </summary>
public class RetryingHttpMessageHandler : DelegatingHandler
{
    private static readonly ResiliencePropertyKey<HttpRequestMessage?> RequestKey = new("StellarSdk.HttpRequest");
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RetryingHttpMessageHandler" /> class.
    /// </summary>
    /// <param name="innerHandler">The inner handler to delegate to.</param>
    /// <param name="options">The resilience options. If null, default options are used.</param>
    /// <exception cref="ArgumentNullException">Thrown when innerHandler is null.</exception>
    public RetryingHttpMessageHandler(HttpMessageHandler innerHandler, HttpResilienceOptions? options = null)
        : base(innerHandler ?? throw new ArgumentNullException(nameof(innerHandler)))
    {
        var opts = options ?? new HttpResilienceOptions();
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

    private static ResiliencePipeline<HttpResponseMessage> BuildPipeline(HttpResilienceOptions options)
    {
        var builder = new ResiliencePipelineBuilder<HttpResponseMessage>();

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
                    var request = args.Context.Properties.GetValue(RequestKey, null);
                    return new ValueTask<bool>(ShouldHandleOutcome(args.Outcome, request, options,
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

                    var retryAfter = args.Outcome.Result?.Headers.RetryAfter;
                    var parsed = RetryAfterParser.ToTimeSpan(retryAfter);
                    if (parsed is { } delay)
                    {
                        // Polly does not apply RetryStrategyOptions.MaxDelay to a value returned from
                        // DelayGenerator, so cap the server-provided Retry-After against MaxDelay here.
                        if (delay > options.MaxDelay)
                        {
                            delay = options.MaxDelay;
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
            // If the request couldn't be observed through the Polly context (e.g. the pipeline was
            // invoked without one), default to allowing the retry — the upstream method check is the
            // safety net, not a hard gate.
            return request == null || options.RetryHttpMethods.Contains(request.Method);
        }

        return false;
    }

    private static bool IsRetriableException(Exception exception, HttpResilienceOptions options,
        bool cancellationRequested)
    {
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

        // TaskCanceledException - only retriable if it's a timeout, not user cancellation
        if (exception is TaskCanceledException)
        {
            // Retry if cancellation token not signaled (likely timeout), otherwise propagate
            return !cancellationRequested;
        }

        return false;
    }
}