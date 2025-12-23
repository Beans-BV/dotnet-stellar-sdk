namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Base class for all challenge validation errors in SEP-0010 authentication.
/// </summary>
public class ChallengeValidationException : WebAuthException
{
    public ChallengeValidationException(string message)
        : base(message)
    {
    }
}

