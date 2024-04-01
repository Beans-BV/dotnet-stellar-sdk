using System;

namespace StellarDotnetSdk.Federation;

public class ConnectionErrorException : Exception
{
    public ConnectionErrorException(string message)
        : base(message)
    {
    }
}