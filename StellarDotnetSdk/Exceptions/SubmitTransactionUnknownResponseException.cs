using System;
using System.Net;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     The exception that is thrown when the Stellar Horizon server returns an unexpected or unrecognized
///     HTTP status code in response to a transaction submission.
/// </summary>
public class SubmitTransactionUnknownResponseException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SubmitTransactionUnknownResponseException" /> class
    ///     with the HTTP status code and response body from the Horizon server.
    /// </summary>
    /// <param name="code">The HTTP status code returned by the Horizon server.</param>
    /// <param name="body">The response body returned by the Horizon server.</param>
    public SubmitTransactionUnknownResponseException(HttpStatusCode code, string body) :
        base($"Unknown response from Horizon - code: {code} - body: {body}")
    {
        Body = body;
        Code = code;
    }

    public HttpStatusCode Code { get; }
    public string Body { get; }
}