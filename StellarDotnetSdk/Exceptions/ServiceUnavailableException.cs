using System;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     The exception that is thrown when the Stellar Horizon server returns an HTTP 503 Service Unavailable response,
///     indicating the server is temporarily overloaded or undergoing maintenance.
/// </summary>
public class ServiceUnavailableException : Exception
{
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

    public int? RetryAfter { get; }
}