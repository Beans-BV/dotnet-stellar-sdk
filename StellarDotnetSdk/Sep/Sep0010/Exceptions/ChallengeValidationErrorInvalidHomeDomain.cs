namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when the first operation's data name does not match expected home domain.
/// </summary>
public class ChallengeValidationErrorInvalidHomeDomain : ChallengeValidationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ChallengeValidationErrorInvalidHomeDomain" /> class.
    /// </summary>
    /// <param name="message">The error message describing the home domain mismatch.</param>
    public ChallengeValidationErrorInvalidHomeDomain(string message)
        : base(message)
    {
    }
}