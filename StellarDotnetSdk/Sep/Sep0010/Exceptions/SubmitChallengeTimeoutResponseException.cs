namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Exception thrown when the token endpoint returns HTTP 504 (Gateway Timeout).
/// </summary>
public class SubmitChallengeTimeoutResponseException : WebAuthException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SubmitChallengeTimeoutResponseException" /> class.
    /// </summary>
    public SubmitChallengeTimeoutResponseException()
        : base("Timeout (HTTP 504).")
    {
    }
}