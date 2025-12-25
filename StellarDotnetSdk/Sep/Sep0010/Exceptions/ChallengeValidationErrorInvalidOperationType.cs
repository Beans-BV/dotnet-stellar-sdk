namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when the challenge contains an operation of incorrect type.
/// </summary>
public class ChallengeValidationErrorInvalidOperationType : ChallengeValidationException
{
    public ChallengeValidationErrorInvalidOperationType(string message)
        : base(message)
    {
    }
}