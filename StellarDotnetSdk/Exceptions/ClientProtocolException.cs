using System;

namespace StellarDotnetSdk.Exceptions;

public class ClientProtocolException : Exception
{
    public ClientProtocolException(string message)
        : base(message)
    {
    }
}