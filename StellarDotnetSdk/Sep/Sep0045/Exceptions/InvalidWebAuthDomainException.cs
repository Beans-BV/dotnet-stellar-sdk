namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when the web auth domain in the SEP-45 challenge does not match the expected value.</summary>
public class InvalidWebAuthDomainException : InvalidSep45ChallengeException
{
    /// <summary>Initializes a new instance of the <see cref="InvalidWebAuthDomainException" /> class.</summary>
    /// <param name="message">The validation error message.</param>
    public InvalidWebAuthDomainException(string message) : base(message) { }
}
