namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when the challenge transaction has invalid time bounds.
/// </summary>
public class ChallengeValidationErrorInvalidTimeBounds : ChallengeValidationException
{
    public ChallengeValidationErrorInvalidTimeBounds(string message)
        : base(message)
    {
    }
}

