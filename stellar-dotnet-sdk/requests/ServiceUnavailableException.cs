using System;

namespace stellar_dotnet_sdk.requests;

public class ServiceUnavailableException : Exception
{
    public ServiceUnavailableException(object? retryAfter = null)
        : base("The server is currently unable to handle the request due to a temporary overloading or maintenance of the server.")
    {
        try
        {
            var retryAfterStringValue = retryAfter?.ToString();
            if (string.IsNullOrWhiteSpace(retryAfterStringValue)) 
                return;

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
