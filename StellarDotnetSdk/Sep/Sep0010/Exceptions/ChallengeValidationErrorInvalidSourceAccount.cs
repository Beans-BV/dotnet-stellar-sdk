namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when an operation has an invalid source account.
/// </summary>
public class ChallengeValidationErrorInvalidSourceAccount : ChallengeValidationException
{
    public ChallengeValidationErrorInvalidSourceAccount(string message)
        : base(message)
    {
    }
}

