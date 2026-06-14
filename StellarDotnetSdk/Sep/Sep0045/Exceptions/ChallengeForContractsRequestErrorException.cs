using System;

namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when the SEP-45 challenge request returns a non-success HTTP status.</summary>
public class ChallengeForContractsRequestErrorException : WebAuthContractException
{
    /// <summary>Initializes a new instance of the <see cref="ChallengeForContractsRequestErrorException" /> class.</summary>
    /// <param name="statusCode">The HTTP status code returned by the server.</param>
    /// <param name="body">The response body returned by the server.</param>
    public ChallengeForContractsRequestErrorException(int statusCode, string body)
        : base($"Challenge request failed with status {statusCode}: {body}")
    {
        StatusCode = statusCode;
        Body = body;
    }

    /// <summary>Initializes a new instance that preserves the underlying transport exception.</summary>
    /// <param name="statusCode">The HTTP status code returned by the server (0 if the request never completed).</param>
    /// <param name="body">The response body or a description of the transport failure.</param>
    /// <param name="innerException">The underlying cause (e.g. socket, TLS, or timeout exception).</param>
    public ChallengeForContractsRequestErrorException(int statusCode, string body, Exception innerException)
        : base($"Challenge request failed with status {statusCode}: {body}", innerException)
    {
        StatusCode = statusCode;
        Body = body;
    }

    /// <summary>The HTTP status code returned by the server.</summary>
    public int StatusCode { get; }

    /// <summary>The response body returned by the server.</summary>
    public string Body { get; }
}
