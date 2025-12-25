namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when the challenge transaction has an invalid sequence number.
/// </summary>
public class ChallengeValidationErrorInvalidSeqNr : ChallengeValidationException
{
    public ChallengeValidationErrorInvalidSeqNr(string message)
        : base(message)
    {
    }
}