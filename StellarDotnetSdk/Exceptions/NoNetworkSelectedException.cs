using System;
using System.Runtime.Serialization;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     The exception that is thrown when an operation requiring a Stellar network (such as signing a transaction)
///     is attempted before a network (e.g., public or testnet) has been configured via <c>Network.Use</c>.
/// </summary>
[Serializable]
public class NoNetworkSelectedException : Exception
{
    public NoNetworkSelectedException()
    {
    }

    public NoNetworkSelectedException(string message) : base(message)
    {
    }

    public NoNetworkSelectedException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected NoNetworkSelectedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}