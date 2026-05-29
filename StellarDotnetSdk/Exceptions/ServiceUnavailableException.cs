using System;
using System.Globalization;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     The exception that is thrown when the Stellar Horizon server returns an HTTP 503 Service Unavailable response,
///     indicating the server is temporarily overloaded or undergoing maintenance.
/// </summary>
public class ServiceUnavailableException : Exception
{
    private readonly object? _retryAfterRaw;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ServiceUnavailableException" /> class.
    /// </summary>
    /// <param name="retryAfter">
    ///     The value of the Retry-After header from the HTTP response. Can be an integer (seconds) or a date-time string.
    /// </param>
    public ServiceUnavailableException(object? retryAfter = null)
        : base(
            "The server is currently unable to handle the request due to a temporary overloading or maintenance of the server.")
    {
        _retryAfterRaw = retryAfter;
        try
        {
            var retryAfterStringValue = retryAfter?.ToString();
            if (string.IsNullOrWhiteSpace(retryAfterStringValue))
            {
                return;
            }

            if (int.TryParse(retryAfterStringValue, out var retryAfterInt))
            {
                RetryAfter = retryAfterInt;
            }
            else if (DateTimeOffset.TryParse(retryAfterStringValue, CultureInfo.InvariantCulture,
                         DateTimeStyles.AssumeUniversal, out var retryAfterDate))
            {
                var seconds = (retryAfterDate - DateTimeOffset.UtcNow).TotalSeconds;
                if (seconds > 0 && seconds <= int.MaxValue)
                {
                    RetryAfter = (int)Math.Ceiling(seconds);
                }
            }
        }
        catch (Exception)
        {
            RetryAfter = null;
        }
    }

    /// <summary>Gets the number of seconds to wait before retrying, parsed from the Retry-After header.</summary>
    public int? RetryAfter { get; }

    /// <summary>
    ///     The <c>Retry-After</c> value parsed as a <see cref="TimeSpan" />, or null if absent or unparseable.
    ///     Supports both the delay-seconds and HTTP-date forms (RFC 7231 §7.1.3) and preserves the full value,
    ///     whereas <see cref="RetryAfter" /> exposes whole seconds only.
    /// </summary>
    public TimeSpan? RetryAfterDelay => RetryAfterParser.Parse(_retryAfterRaw);
}