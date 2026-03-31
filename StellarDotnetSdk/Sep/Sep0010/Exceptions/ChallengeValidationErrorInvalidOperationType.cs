namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when the challenge contains an operation of incorrect type.
/// </summary>
public class ChallengeValidationErrorInvalidOperationType : ChallengeValidationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ChallengeValidationErrorInvalidOperationType" /> class.
    /// </summary>
    /// <param name="message">The error message describing the invalid operation type.</param>
    public ChallengeValidationErrorInvalidOperationType(string message)
        : base(message)
    {
    }
}