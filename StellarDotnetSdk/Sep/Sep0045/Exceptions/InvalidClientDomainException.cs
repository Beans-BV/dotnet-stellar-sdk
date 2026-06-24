namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when the client domain in the SEP-45 challenge is invalid or unexpected.</summary>
public class InvalidClientDomainException : InvalidSep45ChallengeException
{
    /// <summary>Initializes a new instance of the <see cref="InvalidClientDomainException" /> class.</summary>
    /// <param name="message">The validation error message.</param>
    public InvalidClientDomainException(string message) : base(message) { }
}
