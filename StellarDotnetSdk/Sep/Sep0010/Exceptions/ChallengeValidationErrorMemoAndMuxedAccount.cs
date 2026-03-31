namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when both a memo and muxed account (M... address) are present.
/// </summary>
public class ChallengeValidationErrorMemoAndMuxedAccount : ChallengeValidationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ChallengeValidationErrorMemoAndMuxedAccount" /> class.
    /// </summary>
    /// <param name="message">The error message describing the conflict between memo and muxed account.</param>
    public ChallengeValidationErrorMemoAndMuxedAccount(string message)
        : base(message)
    {
    }
}