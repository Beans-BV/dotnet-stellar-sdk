namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Base class for all challenge validation errors in SEP-0010 authentication.
/// </summary>
public class ChallengeValidationException : WebAuthException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ChallengeValidationException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message describing the challenge validation failure.</param>
    public ChallengeValidationException(string message)
        : base(message)
    {
    }
}