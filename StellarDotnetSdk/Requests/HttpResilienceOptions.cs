using System;
using System.Collections.Generic;
using System.Net;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Configuration options for HTTP resilience behavior including retry, circuit breaker, and timeout policies.
/// </summary>
public sealed class HttpResilienceOptions
{
    private int _maxRetryCount = 0;
    private TimeSpan _baseDelay = TimeSpan.FromMilliseconds(200);
    private TimeSpan _maxDelay = TimeSpan.FromSeconds(5);
    private double _failureRatio = 0.5;
    private int _minimumThroughput = 10;
    private TimeSpan _samplingDuration = TimeSpan.FromSeconds(30);
    private TimeSpan _breakDuration = TimeSpan.FromSeconds(30);

    /// <summary>
    ///     Gets or sets the maximum number of retry attempts. Default is 0 (disabled).
    ///     Set to a positive value to enable retries for connection failures only (network errors, DNS failures, etc.).
    ///     HTTP error status codes (4xx/5xx) are never retried automatically.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is negative.</exception>
    public int MaxRetryCount
    {
        get => _maxRetryCount;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "MaxRetryCount cannot be negative.");
            }

            _maxRetryCount = value;
        }
    }

    /// <summary>
    ///     Gets or sets the base delay for exponential backoff. Default is 200ms.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is not positive.</exception>
    public TimeSpan BaseDelay
    {
        get => _baseDelay;
        set
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "BaseDelay must be positive.");
            }

            _baseDelay = value;
            if (value > _maxDelay)
            {
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"BaseDelay ({value.TotalMilliseconds}ms) cannot exceed MaxDelay ({_maxDelay.TotalMilliseconds}ms).");
            }
        }
    }

    /// <summary>
    ///     Gets or sets the maximum delay for exponential backoff. Default is 5 seconds.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is not positive.</exception>
    public TimeSpan MaxDelay
    {
        get => _maxDelay;
        set
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "MaxDelay must be positive.");
            }

            _maxDelay = value;
            if (value < _baseDelay)
            {
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"MaxDelay ({value.TotalMilliseconds}ms) cannot be less than BaseDelay ({_baseDelay.TotalMilliseconds}ms).");
            }
        }
    }

    /// <summary>
    ///     Gets or sets whether to use jitter in the backoff calculation. Default is true.
    /// </summary>
    public bool UseJitter { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether to honor Retry-After headers. Default is true.
    ///     Note: This is only relevant if you implement custom retry logic for HTTP status codes.
    ///     Connection-level retries use exponential backoff.
    /// </summary>
    public bool HonorRetryAfterHeader { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether to enable circuit breaker. Default is false.
    /// </summary>
    public bool EnableCircuitBreaker { get; set; } = false;

    /// <summary>
    ///     Gets or sets the failure ratio threshold for circuit breaker (0.0 to 1.0). Default is 0.5 (50%).
    ///     The circuit will open when the failure ratio exceeds this value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is not between 0.0 and 1.0.</exception>
    public double FailureRatio
    {
        get => _failureRatio;
        set
        {
            if (value < 0.0 || value > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "FailureRatio must be between 0.0 and 1.0.");
            }

            _failureRatio = value;
        }
    }

    /// <summary>
    ///     Gets or sets the minimum throughput required before circuit breaker can trip. Default is 10.
    ///     The circuit breaker will not open unless at least this many requests have been made.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is not positive.</exception>
    public int MinimumThroughput
    {
        get => _minimumThroughput;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "MinimumThroughput must be positive.");
            }

            _minimumThroughput = value;
        }
    }

    /// <summary>
    ///     Gets or sets the sampling duration for circuit breaker. Default is 30 seconds.
    ///     The circuit breaker evaluates failures over this time window.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is not positive.</exception>
    public TimeSpan SamplingDuration
    {
        get => _samplingDuration;
        set
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "SamplingDuration must be positive.");
            }

            _samplingDuration = value;
        }
    }

    /// <summary>
    ///     Gets or sets the duration the circuit breaker stays open before attempting to close. Default is 30 seconds.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is not positive.</exception>
    public TimeSpan BreakDuration
    {
        get => _breakDuration;
        set
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "BreakDuration must be positive.");
            }

            _breakDuration = value;
        }
    }

    /// <summary>
    ///     Gets or sets the per-request timeout. Default is null (no timeout).
    ///     When set, each request will timeout after this duration.
    /// </summary>
    public TimeSpan? RequestTimeout { get; set; } = null;

    /// <summary>
    ///     Gets whether any resilience feature is enabled (retries, circuit breaker, or timeout).
    ///     Returns true if MaxRetryCount is greater than 0, EnableCircuitBreaker is true, or RequestTimeout has a value.
    /// </summary>
    public bool HasAnyResilienceFeatureEnabled =>
        MaxRetryCount > 0 || EnableCircuitBreaker || RequestTimeout.HasValue;

    /// <summary>
    ///     Gets additional exception types to consider as retriable.
    ///     By default, HttpRequestException, TimeoutException, and TaskCanceledException (from timeouts) are retriable.
    ///     Note: HTTP status codes are never retried automatically. Only connection-level failures (exceptions) are retried.
    /// </summary>
    public ISet<Type> AdditionalRetriableExceptionTypes { get; } = new HashSet<Type>();
}

/// <summary>
///     Provides preset configurations for common HTTP resilience scenarios.
/// </summary>
public static class HttpResilienceOptionsPresets
{
    /// <summary>
    ///     Creates default options with retries disabled.
    ///     This is the same as using <c>new HttpResilienceOptions()</c>.
    /// </summary>
    public static HttpResilienceOptions Default() => new();

    /// <summary>
    ///     Creates options with connection retries enabled (similar to OkHttp's retryOnConnectionFailure).
    ///     Retries connection failures (network errors, DNS failures) but not HTTP error status codes.
    /// </summary>
    public static HttpResilienceOptions WithConnectionRetries() => new()
    {
        MaxRetryCount = 3,
        BaseDelay = TimeSpan.FromMilliseconds(200),
        MaxDelay = TimeSpan.FromSeconds(5)
    };

    /// <summary>
    ///     Creates options with retries disabled (default behavior).
    ///     Requests fail immediately on connection failures without retrying.
    /// </summary>
    public static HttpResilienceOptions NoRetry() => new() { MaxRetryCount = 0 };

    /// <summary>
    ///     Creates options tuned for Soroban RPC polling workflows.
    ///     Uses more retries and longer delays for connection failures to accommodate network instability.
    ///     Note: HTTP error status codes are still not retried automatically.
    /// </summary>
    public static HttpResilienceOptions ForSorobanPolling() => new()
    {
        MaxRetryCount = 5,
        BaseDelay = TimeSpan.FromMilliseconds(500),
        MaxDelay = TimeSpan.FromSeconds(15)
    };

    /// <summary>
    ///     Creates options for high-frequency, latency-sensitive use cases (e.g., trading bots).
    ///     Uses fewer retries and shorter delays for connection failures.
    /// </summary>
    public static HttpResilienceOptions LowLatency() => new()
    {
        MaxRetryCount = 1,
        BaseDelay = TimeSpan.FromMilliseconds(50),
        MaxDelay = TimeSpan.FromMilliseconds(200)
    };
}