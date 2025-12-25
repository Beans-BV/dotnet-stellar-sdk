namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Exception thrown when the server's challenge response does not contain a transaction.
/// </summary>
public class MissingTransactionInChallengeResponseException : WebAuthException
{
    public MissingTransactionInChallengeResponseException()
        : base("Missing transaction in challenge response")
    {
    }
}