namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when the server responds with HTTP 504 while submitting the signed SEP-45 challenge.</summary>
public class SubmitSignedChallengeForContractsTimeoutResponseException : WebAuthContractException
{
    /// <summary>Initializes a new instance of the <see cref="SubmitSignedChallengeForContractsTimeoutResponseException" /> class.</summary>
    public SubmitSignedChallengeForContractsTimeoutResponseException()
        : base("Timeout submitting signed challenge (HTTP 504).")
    {
    }
}
