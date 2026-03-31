using System;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     The exception that is thrown when an HTTP client protocol error occurs while communicating with a Stellar server.
/// </summary>
public class ClientProtocolException : Exception
{
    /// <inheritdoc />
    public ClientProtocolException(string message)
        : base(message)
    {
    }
}