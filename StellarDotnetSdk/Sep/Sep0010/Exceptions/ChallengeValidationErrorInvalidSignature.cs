namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when the server's signature on the challenge is invalid or missing.
/// </summary>
public class ChallengeValidationErrorInvalidSignature : ChallengeValidationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ChallengeValidationErrorInvalidSignature" /> class.
    /// </summary>
    /// <param name="message">The error message describing the signature validation failure.</param>
    public ChallengeValidationErrorInvalidSignature(string message)
        : base(message)
    {
    }
}