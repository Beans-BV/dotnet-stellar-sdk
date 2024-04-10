using System;

namespace StellarDotnetSdk.Exceptions;

public class TooManyRequestsException : Exception
{
    public TooManyRequestsException(int? retryAfter)
        : base("The rate limit for the requesting IP address is over its allowed limit.")
    {
        RetryAfter = retryAfter;
    }

    public int? RetryAfter { get; }
}