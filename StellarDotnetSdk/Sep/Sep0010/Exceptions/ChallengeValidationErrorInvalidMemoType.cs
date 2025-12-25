namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when the challenge transaction has an invalid memo type.
/// </summary>
public class ChallengeValidationErrorInvalidMemoType : ChallengeValidationException
{
    public ChallengeValidationErrorInvalidMemoType(string message)
        : base(message)
    {
    }
}