namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when the challenge transaction has an invalid memo type.
/// </summary>
public class ChallengeValidationErrorInvalidMemoType : ChallengeValidationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ChallengeValidationErrorInvalidMemoType" /> class.
    /// </summary>
    /// <param name="message">The error message describing the invalid memo type.</param>
    public ChallengeValidationErrorInvalidMemoType(string message)
        : base(message)
    {
    }
}