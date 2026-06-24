namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when the server returns an error body after the signed SEP-45 challenge is submitted.</summary>
public class SubmitSignedChallengeForContractsErrorResponseException : WebAuthContractException
{
    /// <summary>Initializes a new instance of the <see cref="SubmitSignedChallengeForContractsErrorResponseException" /> class.</summary>
    /// <param name="error">The error message returned by the server.</param>
    public SubmitSignedChallengeForContractsErrorResponseException(string error)
        : base($"Server returned error: {error}")
    {
        Error = error;
    }

    /// <summary>The error message returned by the server.</summary>
    public string Error { get; }
}
