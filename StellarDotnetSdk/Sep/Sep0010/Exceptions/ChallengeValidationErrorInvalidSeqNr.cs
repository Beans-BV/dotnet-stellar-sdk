namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when the challenge transaction has an invalid sequence number.
/// </summary>
public class ChallengeValidationErrorInvalidSeqNr : ChallengeValidationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ChallengeValidationErrorInvalidSeqNr" /> class.
    /// </summary>
    /// <param name="message">The error message describing the invalid sequence number.</param>
    public ChallengeValidationErrorInvalidSeqNr(string message)
        : base(message)
    {
    }
}