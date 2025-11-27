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
///     HTTP message handler that implements retry logic for connection failures (network errors, DNS failures, etc.).
///     Similar to OkHttp's <c>retryOnConnectionFailure(true)</c> - only retries connection-level failures, not HTTP error status codes.
/// </summary>
public class RetryingHttpMessageHandler : DelegatingHandler
{
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

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // No cloning needed - connection failures happen before request is sent
        return await _pipeline.ExecuteAsync(
            async ct => await base.SendAsync(request, ct).ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);
    }

    private static ResiliencePipeline<HttpResponseMessage> BuildPipeline(HttpResilienceOptions options)
    {
        var builder = new ResiliencePipelineBuilder<HttpResponseMessage>();

        // Timeout (outermost - applies to entire operation)
        if (options.RequestTimeout.HasValue)
        {
            builder.AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = options.RequestTimeout.Value
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
                    return new ValueTask<bool>(ShouldHandleOutcome(args.Outcome, options, cancellationRequested));
                }
            });
        }

        // Retry (innermost - executes first)
        // Retry only on connection failures (exceptions), not HTTP status codes
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
                    return new ValueTask<bool>(ShouldHandleOutcome(args.Outcome, options, cancellationRequested));
                }
            };

            builder.AddRetry(retryOptions);
        }

        return builder.Build();
    }

    private static bool ShouldHandleOutcome(
        Outcome<HttpResponseMessage> outcome,
        HttpResilienceOptions options,
        bool cancellationRequested)
    {
        if (outcome.Exception != null)
        {
            return IsRetriableException(outcome.Exception, options, cancellationRequested);
        }

        // Don't retry HTTP status codes - let them propagate
        return false;
    }

    private static bool IsRetriableException(Exception exception, HttpResilienceOptions options, bool cancellationRequested)
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
