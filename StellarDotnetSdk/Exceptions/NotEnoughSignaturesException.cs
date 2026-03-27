using System;
using System.Runtime.Serialization;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     The exception that is thrown when a Stellar transaction does not have enough signatures
///     to meet the required threshold for the operation(s) it contains.
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