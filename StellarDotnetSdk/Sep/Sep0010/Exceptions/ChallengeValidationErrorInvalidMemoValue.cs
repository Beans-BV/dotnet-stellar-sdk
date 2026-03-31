namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when the challenge transaction's memo value does not match expected.
/// </summary>
public class ChallengeValidationErrorInvalidMemoValue : ChallengeValidationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ChallengeValidationErrorInvalidMemoValue" /> class.
    /// </summary>
    /// <param name="message">The error message describing the memo value mismatch.</param>
    public ChallengeValidationErrorInvalidMemoValue(string message)
        : base(message)
    {
    }
}