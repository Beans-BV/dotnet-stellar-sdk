using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

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
    private TimeSpan _maxRetryAfterDelay = TimeSpan.FromMinutes(1);
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

            // The BaseDelay <= MaxDelay relationship is validated when the retry pipeline is built (see
            // RetryingHttpMessageHandler.BuildPipeline), not here, so BaseDelay and MaxDelay can be assigned
            // in any order via an object initializer without a spurious order-dependent validation failure.
            _baseDelay = value;
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

            // Cross-field validation against BaseDelay is deferred to pipeline-build time so the two
            // properties are order-independent in an object initializer (see BaseDelay).
            _maxDelay = value;
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
    ///     Gets or sets the overall operation timeout. Default is null (no timeout).
    ///     When set, the entire send operation — including all retry attempts and their backoff delays — must
    ///     complete within this duration (applied as the outermost resilience strategy), not each attempt individually.
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
    ///     Retries are also gated by <see cref="RetryHttpMethods" />: only requests whose HTTP method is in
    ///     that set are retried on a configured status code.
    /// </summary>
    public ISet<HttpStatusCode> RetryHttpStatusCodes { get; } = new HashSet<HttpStatusCode>();

    /// <summary>
    ///     HTTP methods that may be retried on a configured status code. Default: <c>GET</c>, <c>HEAD</c>,
    ///     <c>OPTIONS</c> — the methods that are safe by RFC 9110. To retry POST (e.g. for
    ///     <c>Server.SubmitTransaction()</c> on Horizon, or every JSON-RPC call on Stellar RPC), add
    ///     <c>HttpMethod.Post</c> explicitly. Methods not in this set are never retried on a status code,
    ///     even if the status code is in <see cref="RetryHttpStatusCodes" />.
    ///     <para>
    ///         This is an explicit whitelist by design: future SDK methods that introduce <c>PUT</c>,
    ///         <c>PATCH</c>, or <c>DELETE</c> are not silently retried unless added here. Some Stellar
    ///         ecosystem endpoints (e.g. SEP-10 <c>POST /auth</c>, SEP-24
    ///         <c>POST /transactions/{deposit,withdraw}/interactive</c>)
    ///         are explicitly non-idempotent per spec, so a blanket "retry everything" policy is unsafe.
    ///     </para>
    /// </summary>
    public ISet<HttpMethod> RetryHttpMethods { get; } = new HashSet<HttpMethod>
    {
        HttpMethod.Get,
        HttpMethod.Head,
        HttpMethod.Options,
    };

    /// <summary>
    ///     When the server responds with a <c>Retry-After</c> header (RFC 7231) on a retried status code,
    ///     use that value as the retry delay (capped by <see cref="MaxRetryAfterDelay" />). Default: true.
    ///     Only takes effect when status-code retries are enabled (see <see cref="RetryHttpStatusCodes" />).
    /// </summary>
    public bool RespectRetryAfter { get; set; } = true;

    /// <summary>
    ///     Upper bound applied to a server-provided <c>Retry-After</c> delay (RFC 7231) when
    ///     <see cref="RespectRetryAfter" /> is true: a longer <c>Retry-After</c> is capped to this value.
    ///     Default is 1 minute.
    ///     <para>
    ///         This is intentionally separate from <see cref="MaxDelay" />, which bounds the exponential
    ///         backoff. Servers commonly ask for tens of seconds on HTTP 429 — longer than a typical backoff
    ///         cap — and honoring that (rather than retrying early against a server that explicitly asked the
    ///         client to wait) is the whole point of <c>Retry-After</c>. Keep this at or above
    ///         <see cref="MaxDelay" />.
    ///     </para>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is not positive.</exception>
    public TimeSpan MaxRetryAfterDelay
    {
        get => _maxRetryAfterDelay;
        set
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "MaxRetryAfterDelay must be positive.");
            }

            _maxRetryAfterDelay = value;
        }
    }
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
    ///     Creates options tuned for Stellar RPC (Soroban). Retries connection failures and transient HTTP
    ///     status codes (408/429/500/502/503/504) on every request — including POST — because Stellar RPC
    ///     routes every JSON-RPC call (read and write) through HTTP POST and every Stellar operation is
    ///     idempotent on the wire (queries are pure reads; <c>sendTransaction</c> is keyed by transaction
    ///     hash plus source-account sequence number, so a resubmit either returns the cached result or fails
    ///     with <c>tx_bad_seq</c> — no double-spend). Uses a higher retry budget (5 attempts) and longer
    ///     delays (up to 15s) suited to long-running polling workflows like <c>getTransaction</c>; one-off
    ///     interactive calls can override <c>MaxRetryCount</c> / <c>MaxDelay</c> for tighter latency.
    /// </summary>
    public static HttpResilienceOptions ForSoroban()
    {
        var options = new HttpResilienceOptions
        {
            MaxRetryCount = 5,
            BaseDelay = TimeSpan.FromMilliseconds(500),
            MaxDelay = TimeSpan.FromSeconds(15),
            RespectRetryAfter = true,
        };
        options.RetryHttpMethods.Add(HttpMethod.Post);
        SeedTransientHttpStatusCodes(options);
        return options;
    }

    /// <summary>
    ///     Deprecated alias for <see cref="ForSoroban" />, retained for source compatibility with releases
    ///     that exposed <c>ForSorobanPolling()</c>. Note that <see cref="ForSoroban" /> additionally retries
    ///     the transient HTTP status codes (408/429/500/502/503/504) on Soroban's POST calls, which the
    ///     original connection-failure-only <c>ForSorobanPolling()</c> did not.
    /// </summary>
    [Obsolete("Renamed to ForSoroban(). ForSoroban() also retries transient HTTP status codes on POST; " +
              "update call sites accordingly.")]
    public static HttpResilienceOptions ForSorobanPolling()
    {
        return ForSoroban();
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
    ///     Creates options tuned for the Stellar Horizon API. Retries connection failures and the transient
    ///     HTTP status codes (408/429/500/502/503/504 — emitted directly by Horizon for rate-limit, timeout,
    ///     ingestion, and overload cases, plus 408/502 from the CDN/LB layer fronting Stellar Foundation
    ///     endpoints) on Horizon's GET queries and POST <c>SubmitTransaction()</c>. Up to 3 retries with
    ///     exponential backoff (200ms-5s) and jitter; honors the <c>Retry-After</c> header (which Horizon
    ///     sends on 429).
    ///     <para>
    ///         Retrying <c>SubmitTransaction()</c> is safe on Stellar: each envelope is uniquely keyed by
    ///         transaction hash plus source-account sequence number, so a resubmit either returns the cached
    ///         server result or fails with <c>tx_bad_seq</c> — there is no double-spend window. If your tx
    ///         had already committed when the transient failure occurred, the retry surfaces <c>tx_bad_seq</c>
    ///         and your code should look up the transaction by its hash to recover the original result.
    ///     </para>
    ///     <para>
    ///         <b>Scope:</b> this preset is for <c>Server</c> (Horizon) only. Do NOT wire it into SEP service
    ///         clients (<c>ClientWebAuth</c>, <c>InteractiveService</c>, <c>TransferServerService</c>):
    ///         SEP-10 <c>POST /auth</c> consumes a one-shot challenge, SEP-24
    ///         <c>POST /transactions/{deposit,withdraw}/interactive</c>
    ///         creates a fresh transaction row per call, and SEP-6 <c>PATCH /transactions/{id}</c> is not in the
    ///         official spec. For SEP clients, use <see cref="WithConnectionRetries" /> or build a custom
    ///         <see cref="HttpResilienceOptions" /> whose <see cref="HttpResilienceOptions.RetryHttpMethods" />
    ///         only contains <c>GET</c>/<c>HEAD</c>/<c>OPTIONS</c>.
    ///     </para>
    ///     <para>
    ///         For Stellar RPC (Soroban) clients, prefer <see cref="ForSoroban" />.
    ///     </para>
    /// </summary>
    public static HttpResilienceOptions ForHorizon()
    {
        var options = new HttpResilienceOptions
        {
            MaxRetryCount = 3,
            BaseDelay = TimeSpan.FromMilliseconds(200),
            MaxDelay = TimeSpan.FromSeconds(5),
            UseJitter = true,
            RespectRetryAfter = true,
        };
        options.RetryHttpMethods.Add(HttpMethod.Post);
        SeedTransientHttpStatusCodes(options);
        return options;
    }

    private static void SeedTransientHttpStatusCodes(HttpResilienceOptions options)
    {
        options.RetryHttpStatusCodes.Add(HttpStatusCode.RequestTimeout);
        options.RetryHttpStatusCodes.Add(HttpStatusCode.TooManyRequests);
        options.RetryHttpStatusCodes.Add(HttpStatusCode.InternalServerError);
        options.RetryHttpStatusCodes.Add(HttpStatusCode.BadGateway);
        options.RetryHttpStatusCodes.Add(HttpStatusCode.ServiceUnavailable);
        options.RetryHttpStatusCodes.Add(HttpStatusCode.GatewayTimeout);
    }
}