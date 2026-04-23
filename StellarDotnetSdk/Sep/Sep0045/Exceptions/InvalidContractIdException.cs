namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when a contract id in the SEP-45 challenge is invalid or unexpected.</summary>
public class InvalidContractIdException : InvalidSep45ChallengeException
{
    /// <summary>Initializes a new instance of the <see cref="InvalidContractIdException" /> class.</summary>
    /// <param name="message">The validation error message.</param>
    public InvalidContractIdException(string message) : base(message) { }
}
