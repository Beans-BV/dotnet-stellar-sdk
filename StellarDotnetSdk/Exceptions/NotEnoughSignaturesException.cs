using System;
using System.Runtime.Serialization;

namespace StellarDotnetSdk.Exceptions;

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