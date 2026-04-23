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

    /// <summary>The HTTP status code returned by the server.</summary>
    public int StatusCode { get; }

    /// <summary>The response body returned by the server.</summary>
    public string Body { get; }
}
