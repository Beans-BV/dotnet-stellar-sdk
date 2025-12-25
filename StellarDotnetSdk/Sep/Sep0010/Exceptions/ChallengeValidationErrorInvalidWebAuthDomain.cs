namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when the web_auth_domain value does not match the auth endpoint domain.
/// </summary>
public class ChallengeValidationErrorInvalidWebAuthDomain : ChallengeValidationException
{
    public ChallengeValidationErrorInvalidWebAuthDomain(string message)
        : base(message)
    {
    }
}