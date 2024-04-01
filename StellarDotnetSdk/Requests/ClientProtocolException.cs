using System;

namespace StellarDotnetSdk.Requests;

public class ClientProtocolException : Exception
{
    public ClientProtocolException(string message)
        : base(message)
    {
    }
}