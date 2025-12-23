namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when both a memo and muxed account (M... address) are present.
/// </summary>
public class ChallengeValidationErrorMemoAndMuxedAccount : ChallengeValidationException
{
    public ChallengeValidationErrorMemoAndMuxedAccount(string message)
        : base(message)
    {
    }
}

