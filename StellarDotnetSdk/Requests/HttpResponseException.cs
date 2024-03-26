using System;

namespace StellarDotnetSdk.Requests;

public class HttpResponseException : Exception
{
    public HttpResponseException(int statusCode, string s)
        : base(s)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; set; }
}