using System;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Represents an error returned by a Horizon server with an HTTP status code of 300 or higher.
/// </summary>
public class HttpResponseException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpResponseException" /> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code returned by the server.</param>
    /// <param name="s">The error message or reason phrase.</param>
    public HttpResponseException(int statusCode, string s)
        : base(s)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    ///     Gets the HTTP status code returned by the Horizon server.
    /// </summary>
    public int StatusCode { get; }
}