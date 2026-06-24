namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when the home domain in the SEP-45 challenge does not match the expected value.</summary>
public class InvalidHomeDomainException : InvalidSep45ChallengeException
{
    /// <summary>Initializes a new instance of the <see cref="InvalidHomeDomainException" /> class.</summary>
    /// <param name="message">The validation error message.</param>
    public InvalidHomeDomainException(string message) : base(message) { }
}
