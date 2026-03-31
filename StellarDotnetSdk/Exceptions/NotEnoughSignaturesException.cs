using System;
using System.Runtime.Serialization;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     The exception that is thrown when a Stellar transaction has no signatures
///     and at least one is required to produce a valid transaction envelope.
/// </summary>
[Serializable]
public class NotEnoughSignaturesException : Exception
{
    public NotEnoughSignaturesException()
    {
    }

    public NotEnoughSignaturesException(string message) : base(message)
    {
    }

    public NotEnoughSignaturesException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected NotEnoughSignaturesException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}