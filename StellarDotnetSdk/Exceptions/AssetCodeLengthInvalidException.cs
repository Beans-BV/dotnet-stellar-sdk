using System;
using System.Runtime.Serialization;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     The exception that is thrown when a Stellar asset code exceeds the allowed length
///     (1-4 characters for AlphaNum4 or 5-12 characters for AlphaNum12).
/// </summary>
[Serializable]
public class AssetCodeLengthInvalidException : Exception
{
    /// <inheritdoc />
    public AssetCodeLengthInvalidException()
    {
    }

    /// <inheritdoc />
    public AssetCodeLengthInvalidException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public AssetCodeLengthInvalidException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected AssetCodeLengthInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}