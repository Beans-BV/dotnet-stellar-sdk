using System;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Represents an error returned by a Horizon server with an HTTP status code of 300 or higher.
/// </summary>
public class HttpResponseException : Exception
{
    public HttpResponseException(int statusCode, string s)
        : base(s)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}