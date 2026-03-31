namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when an operation has an invalid source account.
/// </summary>
public class ChallengeValidationErrorInvalidSourceAccount : ChallengeValidationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ChallengeValidationErrorInvalidSourceAccount" /> class.
    /// </summary>
    /// <param name="message">The error message describing the invalid source account.</param>
    public ChallengeValidationErrorInvalidSourceAccount(string message)
        : base(message)
    {
    }
}