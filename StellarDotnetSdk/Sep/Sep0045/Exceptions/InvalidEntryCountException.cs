namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when the number of authorization entries in the challenge does not match the expected count.</summary>
public class InvalidEntryCountException : InvalidSep45ChallengeException
{
    /// <summary>Initializes a new instance of the <see cref="InvalidEntryCountException" /> class.</summary>
    /// <param name="message">The validation error message.</param>
    public InvalidEntryCountException(string message) : base(message) { }
}
