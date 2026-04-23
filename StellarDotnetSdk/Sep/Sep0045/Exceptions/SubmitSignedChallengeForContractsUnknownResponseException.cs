namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when the server returns an unexpected HTTP status while submitting the signed SEP-45 challenge.</summary>
public class SubmitSignedChallengeForContractsUnknownResponseException : WebAuthContractException
{
    /// <summary>Initializes a new instance of the <see cref="SubmitSignedChallengeForContractsUnknownResponseException" /> class.</summary>
    /// <param name="statusCode">The HTTP status code returned by the server.</param>
    /// <param name="body">The response body returned by the server.</param>
    public SubmitSignedChallengeForContractsUnknownResponseException(int statusCode, string body)
        : base($"Unknown response status {statusCode}: {body}")
    {
        StatusCode = statusCode;
        Body = body;
    }

    /// <summary>The HTTP status code returned by the server.</summary>
    public int StatusCode { get; }

    /// <summary>The response body returned by the server.</summary>
    public string Body { get; }
}
