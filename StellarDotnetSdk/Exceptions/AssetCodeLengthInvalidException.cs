using System;
using System.Runtime.Serialization;

namespace StellarDotnetSdk.Exceptions;

[Serializable]
public class AssetCodeLengthInvalidException : Exception
{
    public AssetCodeLengthInvalidException()
    {
    }

    public AssetCodeLengthInvalidException(string message) : base(message)
    {
    }

    public AssetCodeLengthInvalidException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected AssetCodeLengthInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}