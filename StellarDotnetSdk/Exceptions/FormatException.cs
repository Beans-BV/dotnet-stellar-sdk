using System;
using System.Runtime.Serialization;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     The exception that is thrown when the format of a Stellar SDK value (such as a key, address, or encoded data) is
///     invalid.
/// </summary>
public class FormatException : Exception
{
    /// <inheritdoc />
    public FormatException()
    {
    }

    /// <inheritdoc />
    public FormatException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public FormatException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected FormatException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}