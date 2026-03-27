using System;
using System.Runtime.Serialization;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     The exception that is thrown when a Stellar transaction has more signatures than the maximum
///     number allowed (20 signatures per transaction).
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