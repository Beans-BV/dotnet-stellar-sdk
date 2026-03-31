using System;
using System.Runtime.Serialization;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     The exception that is thrown when a transaction memo exceeds the maximum allowed length
///     (28 bytes for text memos, 32 bytes for hash and return memos).
/// </summary>
[Serializable]
public class MemoTooLongException : Exception
{
    /// <inheritdoc />
    public MemoTooLongException()
    {
    }

    /// <inheritdoc />
    public MemoTooLongException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public MemoTooLongException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected MemoTooLongException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}