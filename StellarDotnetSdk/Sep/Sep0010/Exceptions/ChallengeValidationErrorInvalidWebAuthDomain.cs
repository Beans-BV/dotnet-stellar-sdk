namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when the web_auth_domain value does not match the auth endpoint domain.
/// </summary>
public class ChallengeValidationErrorInvalidWebAuthDomain : ChallengeValidationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ChallengeValidationErrorInvalidWebAuthDomain" /> class.
    /// </summary>
    /// <param name="message">The error message describing the web_auth_domain mismatch.</param>
    public ChallengeValidationErrorInvalidWebAuthDomain(string message)
        : base(message)
    {
    }
}