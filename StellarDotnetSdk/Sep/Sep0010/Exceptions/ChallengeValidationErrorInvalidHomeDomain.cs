namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when the first operation's data name does not match expected home domain.
/// </summary>
public class ChallengeValidationErrorInvalidHomeDomain : ChallengeValidationException
{
    public ChallengeValidationErrorInvalidHomeDomain(string message)
        : base(message)
    {
    }
}

