namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when the server signature on the SEP-45 challenge fails verification.</summary>
public class InvalidServerSignatureException : InvalidSep45ChallengeException
{
    /// <summary>Initializes a new instance of the <see cref="InvalidServerSignatureException" /> class.</summary>
    /// <param name="message">The validation error message.</param>
    public InvalidServerSignatureException(string message) : base(message) { }
}
