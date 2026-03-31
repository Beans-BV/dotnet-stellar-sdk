using System;
using System.Runtime.Serialization;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     The exception that is thrown when attempting to create an unsigned transaction envelope
///     from a transaction that already has signatures.
/// </summary>
[Serializable]
public class TooManySignaturesException : Exception
{
    public TooManySignaturesException()
    {
    }

    public TooManySignaturesException(string message) : base(message)
    {
    }

    public TooManySignaturesException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected TooManySignaturesException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}