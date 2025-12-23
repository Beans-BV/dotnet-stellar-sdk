namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when the server's signature on the challenge is invalid or missing.
/// </summary>
public class ChallengeValidationErrorInvalidSignature : ChallengeValidationException
{
    public ChallengeValidationErrorInvalidSignature(string message)
        : base(message)
    {
    }
}

