using System;
using System.Collections.Generic;
using System.Net;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Configuration options for HTTP resilience behavior including retry, circuit breaker, and timeout policies.
/// </summary>
public sealed class HttpResilienceOptions
{
    private TimeSpan _baseDelay = TimeSpan.FromMilliseconds(200);
    private TimeSpan _breakDuration = TimeSpan.FromSeconds(30);
    private double _failureRatio = 0.5;
    private TimeSpan _maxDelay = TimeSpan.FromSeconds(5);
    private int _maxRetryCount;
    private int _minimumThroughput = 10;
    private TimeSpan _samplingDuration = TimeSpan.FromSeconds(30);

    /// <summary>
    ///     Gets or sets the maximum number of retry attempts. Default is 0 (disabled).
    ///     Set to a positive value to enable retries for connection failures (network errors, DNS failures, etc.).
    ///     HTTP error status codes are retried only when added to <see cref="RetryHttpStatusCodes" />.
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
    ///     Gets whether any resilience feature is enabled (retries, circuit breaker, timeout, or status-code retries).
    ///     Returns true if MaxRetryCount is greater than 0, EnableCircuitBreaker is true, RequestTimeout has a value,
    ///     or RetryHttpStatusCodes is non-empty.
    /// </summary>
    public bool HasAnyResilienceFeatureEnabled =>
        MaxRetryCount > 0 || EnableCircuitBreaker || RequestTimeout.HasValue || RetryHttpStatusCodes.Count > 0;

    /// <summary>
    ///     Additional exception types to retry, in addition to the defaults (
    ///     <see cref="System.Net.Http.HttpRequestException" />,
    ///     <see cref="TimeoutException" />, and <see cref="System.Threading.Tasks.TaskCanceledException" /> when not
    ///     user-cancelled).
    ///     <para>
    ///         <b>Scope limitation:</b> This applies only to exceptions thrown from within the HTTP message handler
    ///         chain (i.e., from <c>HttpClient.SendAsync</c>). It does <i>not</i> apply to exceptions thrown by
    ///         <c>ResponseHandler</c> after the response is received — including <c>TooManyRequestsException</c>,
    ///         <c>ServiceUnavailableException</c>, and <c>HttpResponseException</c>. To retry those, use
    ///         <see cref="RetryHttpStatusCodes" /> instead — that mechanism inspects the response before it is
    ///         translated into a typed exception.
    ///     </para>
    /// </summary>
    public ISet<Type> AdditionalRetriableExceptionTypes { get; } = new HashSet<Type>();

    /// <summary>
    ///     HTTP status codes that should trigger a retry when received as a response. Default: empty (no status-code retries).
    ///     When non-empty and <see cref="MaxRetryCount" /> is greater than 0, the retry pipeline observes
    ///     <see cref="System.Net.Http.HttpResponseMessage.StatusCode" /> and retries on match.
    ///     Retries are gated by <see cref="RetryUnsafeHttpMethods" />: by default, only safe HTTP methods
    ///     (GET, HEAD, OPTIONS) are retried.
    /// </summary>
    public ISet<HttpStatusCode> RetryHttpStatusCodes { get; } = new HashSet<HttpStatusCode>();

    /// <summary>
    ///     When true, status-code retries (see <see cref="RetryHttpStatusCodes" />) apply to all HTTP methods,
    ///     including unsafe ones (POST, PUT, PATCH, DELETE). Default: false — only safe methods (GET, HEAD, OPTIONS)
    ///     are retried. Set to true only if your endpoint is genuinely idempotent or tolerates duplicates
    ///     (e.g., via idempotency keys).
    /// </summary>
    public bool RetryUnsafeHttpMethods { get; set; } = false;

    /// <summary>
    ///     When the server responds with a <c>Retry-After</c> header (RFC 7231) on a retried status code,
    ///     use that value as the retry delay (capped by <see cref="MaxDelay" />). Default: true.
    ///     Only takes effect when status-code retries are enabled (see <see cref="RetryHttpStatusCodes" />).
    /// </summary>
    public bool RespectRetryAfter { get; set; } = true;
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
    public static HttpResilienceOptions Default()
    {
        return new HttpResilienceOptions();
    }

    /// <summary>
    ///     Creates options with connection retries enabled (similar to OkHttp's retryOnConnectionFailure).
    ///     Retries connection failures (network errors, DNS failures) but not HTTP error status codes.
    /// </summary>
    public static HttpResilienceOptions WithConnectionRetries()
    {
        return new HttpResilienceOptions
        {
            MaxRetryCount = 3,
            BaseDelay = TimeSpan.FromMilliseconds(200),
            MaxDelay = TimeSpan.FromSeconds(5),
        };
    }

    /// <summary>
    ///     Creates options with retries disabled (default behavior).
    ///     Requests fail immediately on connection failures without retrying.
    /// </summary>
    public static HttpResilienceOptions NoRetry()
    {
        return new HttpResilienceOptions { MaxRetryCount = 0 };
    }

    /// <summary>
    ///     Creates options tuned for Stellar RPC polling workflows.
    ///     Retries connection failures and the common transient HTTP status codes (408/429/500/502/503/504)
    ///     for safe methods, with a higher retry budget and longer delays suited to long-running operations.
    /// </summary>
    public static HttpResilienceOptions ForSorobanPolling()
    {
        var options = new HttpResilienceOptions
        {
            MaxRetryCount = 5,
            BaseDelay = TimeSpan.FromMilliseconds(500),
            MaxDelay = TimeSpan.FromSeconds(15),
            RespectRetryAfter = true,
        };
        SeedStandardRetryStatusCodes(options);
        return options;
    }

    /// <summary>
    ///     Creates options for high-frequency, latency-sensitive use cases (e.g., trading bots).
    ///     Uses fewer retries and shorter delays for connection failures.
    /// </summary>
    public static HttpResilienceOptions LowLatency()
    {
        return new HttpResilienceOptions
        {
            MaxRetryCount = 1,
            BaseDelay = TimeSpan.FromMilliseconds(50),
            MaxDelay = TimeSpan.FromMilliseconds(200),
        };
    }

    /// <summary>
    ///     Creates options matching the .NET industry standard
    ///     (<c>Microsoft.Extensions.Http.Resilience.AddStandardResilienceHandler</c>):
    ///     retries 408/429/500/502/503/504 for safe HTTP methods, 3 attempts with exponential backoff
    ///     (200ms-5s) and jitter, honors the <c>Retry-After</c> header. POST/PUT/PATCH/DELETE are NOT
    ///     retried by default; set <see cref="HttpResilienceOptions.RetryUnsafeHttpMethods" /> to true
    ///     to opt in. Recommended for general-purpose Horizon/Soroban clients.
    /// </summary>
    public static HttpResilienceOptions WithStandardRetries()
    {
        var options = new HttpResilienceOptions
        {
            MaxRetryCount = 3,
            BaseDelay = TimeSpan.FromMilliseconds(200),
            MaxDelay = TimeSpan.FromSeconds(5),
            UseJitter = true,
            RespectRetryAfter = true,
        };
        SeedStandardRetryStatusCodes(options);
        return options;
    }

    private static void SeedStandardRetryStatusCodes(HttpResilienceOptions options)
    {
        options.RetryHttpStatusCodes.Add(HttpStatusCode.RequestTimeout);
        options.RetryHttpStatusCodes.Add(HttpStatusCode.TooManyRequests);
        options.RetryHttpStatusCodes.Add(HttpStatusCode.InternalServerError);
        options.RetryHttpStatusCodes.Add(HttpStatusCode.BadGateway);
        options.RetryHttpStatusCodes.Add(HttpStatusCode.ServiceUnavailable);
        options.RetryHttpStatusCodes.Add(HttpStatusCode.GatewayTimeout);
    }
}