using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     HTTP message handler that implements retry logic with exponential backoff for transient failures using Polly.
/// </summary>
public class RetryingHttpMessageHandler : DelegatingHandler
{
    private static readonly HttpStatusCode[] DefaultRetriableStatusCodes =
    {
        HttpStatusCode.RequestTimeout, // 408
        (HttpStatusCode)425, // TooEarly - not available in all .NET versions
        HttpStatusCode.TooManyRequests, // 429
        HttpStatusCode.InternalServerError, // 500
        HttpStatusCode.BadGateway, // 502
        HttpStatusCode.ServiceUnavailable, // 503
        HttpStatusCode.GatewayTimeout // 504
    };

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
        return await _pipeline.ExecuteAsync(
            async ct =>
            {
                var response = await base.SendAsync(request, ct).ConfigureAwait(false);
                return response;
            },
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
                ShouldHandle = CreateCircuitBreakerShouldHandle(options)
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
                ShouldHandle = CreateRetryShouldHandle(options)
            };

            // Custom delay generator to honor Retry-After header
            if (options.HonorRetryAfterHeader)
            {
                retryOptions.DelayGenerator = CreateDelayGenerator(options);
            }
            else
            {
                retryOptions.Delay = options.BaseDelay;
                retryOptions.MaxDelay = options.MaxDelay;
            }

            builder.AddRetry(retryOptions);
        }

        return builder.Build();
    }

    private static Func<CircuitBreakerPredicateArguments<HttpResponseMessage>, ValueTask<bool>> CreateCircuitBreakerShouldHandle(
        HttpResilienceOptions options)
    {
        return args =>
        {
            var cancellationRequested = args.Context.CancellationToken.IsCancellationRequested;
            return new ValueTask<bool>(ShouldHandleOutcome(args.Outcome, options, cancellationRequested));
        };
    }

    private static Func<RetryPredicateArguments<HttpResponseMessage>, ValueTask<bool>> CreateRetryShouldHandle(
        HttpResilienceOptions options)
    {
        return args =>
        {
            var cancellationRequested = args.Context.CancellationToken.IsCancellationRequested;
            return new ValueTask<bool>(ShouldHandleOutcome(args.Outcome, options, cancellationRequested));
        };
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

        if (outcome.Result != null)
        {
            return IsRetriableStatusCode(outcome.Result.StatusCode, options);
        }

        return false;
    }

    private static Func<RetryDelayGeneratorArguments<HttpResponseMessage>, ValueTask<TimeSpan?>> CreateDelayGenerator(
        HttpResilienceOptions options)
    {
        return args =>
        {
            // Check for Retry-After header in the response
            if (args.Outcome.Result != null)
            {
                var retryAfterDelay = ParseRetryAfterHeader(args.Outcome.Result, options);
                if (retryAfterDelay.HasValue)
                {
                    return new ValueTask<TimeSpan?>(retryAfterDelay.Value);
                }
            }

            // Fall back to exponential backoff
            var exponentialDelay = CalculateExponentialDelay(args.AttemptNumber, options);
            return new ValueTask<TimeSpan?>(exponentialDelay);
        };
    }

    private static TimeSpan? ParseRetryAfterHeader(HttpResponseMessage response, HttpResilienceOptions options)
    {
        if (!response.Headers.TryGetValues("Retry-After", out var retryAfterValues))
        {
            return null;
        }

        var retryAfterValue = retryAfterValues.FirstOrDefault();
        if (string.IsNullOrEmpty(retryAfterValue))
        {
            return null;
        }

        // Try parsing as integer (seconds)
        if (int.TryParse(retryAfterValue, out var seconds))
        {
            var delay = TimeSpan.FromSeconds(seconds);
            return TimeSpan.FromMilliseconds(Math.Min((int)delay.TotalMilliseconds, (int)options.MaxDelay.TotalMilliseconds));
        }

        // Try parsing as DateTime (RFC 7231 / HTTP-date format)
        if (DateTimeOffset.TryParse(retryAfterValue, out var retryDate))
        {
            var retryDelay = retryDate - DateTimeOffset.UtcNow;
            var delayMs = Math.Min((int)retryDelay.TotalMilliseconds, (int)options.MaxDelay.TotalMilliseconds);
            return TimeSpan.FromMilliseconds(Math.Max(0, delayMs));
        }

        return null;
    }

    private static TimeSpan CalculateExponentialDelay(int attemptNumber, HttpResilienceOptions options)
    {
        // Exponential backoff: baseDelay * 2^attempt
        // Use Math.Min to prevent overflow before multiplication
        var exponentialDelayMs = Math.Min(
            options.BaseDelay.TotalMilliseconds * Math.Pow(2, attemptNumber),
            options.MaxDelay.TotalMilliseconds);

        var delay = TimeSpan.FromMilliseconds(exponentialDelayMs);

        // Apply jitter if enabled (random value between 0.8 and 1.2 of the delay)
        if (options.UseJitter)
        {
            var jitterFactor = 0.8 + (Random.Shared.NextDouble() * 0.4); // 0.8 to 1.2
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * jitterFactor);
        }

        return TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds, options.MaxDelay.TotalMilliseconds));
    }

    private static bool IsRetriableStatusCode(HttpStatusCode statusCode, HttpResilienceOptions options)
    {
        if (Array.IndexOf(DefaultRetriableStatusCodes, statusCode) >= 0)
        {
            return true;
        }

        return options.AdditionalRetriableStatusCodes.Contains(statusCode);
    }

    private static bool IsRetriableException(Exception exception, HttpResilienceOptions options, bool cancellationRequested)
    {
        // Check additional retriable exception types first
        var exceptionType = exception.GetType();
        if (options.AdditionalRetriableExceptionTypes.Any(t => t.IsAssignableFrom(exceptionType)))
        {
            return true;
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
