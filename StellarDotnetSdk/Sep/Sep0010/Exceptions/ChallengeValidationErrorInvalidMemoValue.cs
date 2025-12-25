namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when the challenge transaction's memo value does not match expected.
/// </summary>
public class ChallengeValidationErrorInvalidMemoValue : ChallengeValidationException
{
    public ChallengeValidationErrorInvalidMemoValue(string message)
        : base(message)
    {
    }
}