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
    public MemoTooLongException()
    {
    }

    public MemoTooLongException(string message) : base(message)
    {
    }

    public MemoTooLongException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected MemoTooLongException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}