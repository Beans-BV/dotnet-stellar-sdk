using System;

namespace StellarDotnetSdk.Exceptions;

public class TooManyRequestsException : Exception
{
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

    public int? RetryAfter { get; }
}