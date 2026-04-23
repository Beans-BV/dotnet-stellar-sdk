namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when the SEP-45 challenge response does not contain authorization_entries.</summary>
public class MissingAuthorizationEntriesInChallengeResponseException : WebAuthContractException
{
    /// <summary>Initializes a new instance of the <see cref="MissingAuthorizationEntriesInChallengeResponseException" /> class.</summary>
    public MissingAuthorizationEntriesInChallengeResponseException()
        : base("Challenge response is missing authorization_entries")
    {
    }
}
