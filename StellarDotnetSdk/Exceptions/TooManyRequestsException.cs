using System;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     The exception that is thrown when the Stellar Horizon server returns an HTTP 429 Too Many Requests response,
///     indicating the client has exceeded the rate limit for its IP address.
/// </summary>
public class TooManyRequestsException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TooManyRequestsException" /> class.
    /// </summary>
    /// <param name="retryAfter">
    ///     The value of the Retry-After header from the HTTP response. Can be an int/long (delay-seconds), a
    ///     string in delay-seconds or HTTP-date (RFC 1123, RFC 850, asctime, or ISO 8601) form, or a typed
    ///     TimeSpan, DateTime, DateTimeOffset, or RetryConditionHeaderValue; any other object is parsed from
    ///     its ToString() form.
    ///     Non-positive or unparseable values leave both properties null.
    /// </param>
    public TooManyRequestsException(object? retryAfter = null)
        : base("The rate limit for the requesting IP address is over its allowed limit.")
    {
        TimeSpan? delay = null;
        try
        {
            // Parse the typed value directly; fall back to its string form so callers passing any other
            // object keep the pre-existing ToString-based behavior.
            delay = RetryAfterParser.Parse(retryAfter) ?? RetryAfterParser.Parse(retryAfter?.ToString());
        }
        catch (Exception)
        {
            // A throwing ToString() must not break exception construction; treat as no Retry-After.
        }

        RetryAfterDelay = delay;
        if (delay is { } d && d.TotalSeconds <= int.MaxValue)
        {
            RetryAfter = (int)Math.Ceiling(d.TotalSeconds);
        }
    }

    /// <summary>Gets the number of seconds to wait before retrying, parsed from the Retry-After header.</summary>
    public int? RetryAfter { get; }

    /// <summary>
    ///     The <c>Retry-After</c> value parsed as a <see cref="TimeSpan" />, or null if absent or unparseable.
    ///     Supports both the delay-seconds and HTTP-date forms (RFC 7231 §7.1.3) and preserves the full value,
    ///     whereas <see cref="RetryAfter" /> exposes whole seconds only. Parsed once when the exception is
    ///     created; for the HTTP-date form this is the delay that remained at that moment.
    /// </summary>
    public TimeSpan? RetryAfterDelay { get; }
}