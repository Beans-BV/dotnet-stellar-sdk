namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when the invoked function name in the SEP-45 challenge is not recognized.</summary>
public class InvalidFunctionNameException : InvalidSep45ChallengeException
{
    /// <summary>Initializes a new instance of the <see cref="InvalidFunctionNameException" /> class.</summary>
    /// <param name="message">The validation error message.</param>
    public InvalidFunctionNameException(string message) : base(message) { }
}
