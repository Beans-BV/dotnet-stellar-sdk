using System;
using System.Runtime.Serialization;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     The exception that is thrown when the format of a Stellar SDK value (such as a key, address, or encoded data) is
///     invalid.
/// </summary>
public class FormatException : Exception
{
    public FormatException()
    {
    }

    public FormatException(string message) : base(message)
    {
    }

    public FormatException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected FormatException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}