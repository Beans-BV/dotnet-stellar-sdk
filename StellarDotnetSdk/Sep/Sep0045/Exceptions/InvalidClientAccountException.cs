namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when the client account in the SEP-45 challenge is invalid or unexpected.</summary>
public class InvalidClientAccountException : InvalidSep45ChallengeException
{
    /// <summary>Initializes a new instance of the <see cref="InvalidClientAccountException" /> class.</summary>
    /// <param name="message">The validation error message.</param>
    public InvalidClientAccountException(string message) : base(message) { }
}
