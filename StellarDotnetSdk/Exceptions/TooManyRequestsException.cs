using System;

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
    ///     The value of the Retry-After header from the HTTP response. Can be an integer (seconds) or a date-time string.
    /// </param>
    public TooManyRequestsException(object? retryAfter = null)
        : base("The rate limit for the requesting IP address is over its allowed limit.")
    {
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
            else if (DateTime.TryParse(retryAfterStringValue, out var retryAfterDateTime))
            {
                RetryAfter = (retryAfterDateTime - DateTime.UtcNow).Seconds;
            }
        }
        catch (Exception)
        {
            RetryAfter = null;
        }
    }

    /// <summary>Gets the number of seconds to wait before retrying, parsed from the Retry-After header.</summary>
    public int? RetryAfter { get; }
}