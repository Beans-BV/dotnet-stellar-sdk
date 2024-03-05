using System;

namespace stellar_dotnet_sdk.requests;

public class TooManyRequestsException : Exception
{
    public TooManyRequestsException(int retryAfter)
        : base("The rate limit for the requesting IP address is over its allowed limit.")
    {
        RetryAfter = retryAfter;
    }

    public int RetryAfter { get; set; }
}