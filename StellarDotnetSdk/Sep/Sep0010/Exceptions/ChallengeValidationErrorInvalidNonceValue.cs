namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when the challenge transaction's first operation has an invalid nonce value.
///     According to SEP-0010, the nonce value must be exactly 64 bytes (48-byte random string base64-encoded).
/// </summary>
public class ChallengeValidationErrorInvalidNonceValue : ChallengeValidationException
{
    public ChallengeValidationErrorInvalidNonceValue(string message)
        : base(message)
    {
    }
}