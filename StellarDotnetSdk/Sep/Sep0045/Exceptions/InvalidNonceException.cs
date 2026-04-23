namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when the SEP-45 challenge nonce is invalid.</summary>
public class InvalidNonceException : InvalidSep45ChallengeException
{
    /// <summary>Initializes a new instance of the <see cref="InvalidNonceException" /> class.</summary>
    /// <param name="message">The validation error message.</param>
    public InvalidNonceException(string message) : base(message) { }
}
