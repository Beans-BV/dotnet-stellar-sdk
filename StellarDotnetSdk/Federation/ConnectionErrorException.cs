using System;

namespace StellarDotnetSdk.Federation;

/// <summary>
///     Thrown when a connection error occurs while communicating with a federation server.
/// </summary>
public class ConnectionErrorException : Exception
{
    public ConnectionErrorException(string message)
        : base(message)
    {
    }
}