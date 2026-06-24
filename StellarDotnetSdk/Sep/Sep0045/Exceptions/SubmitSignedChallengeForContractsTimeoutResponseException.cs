using System;

namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>
///     Thrown when submitting the signed SEP-45 challenge times out — either the server responded with
///     HTTP 504, or the request timed out client-side before a response was received.
/// </summary>
public class SubmitSignedChallengeForContractsTimeoutResponseException : WebAuthContractException
{
    /// <summary>Initializes a new instance of the <see cref="SubmitSignedChallengeForContractsTimeoutResponseException" /> class.</summary>
    public SubmitSignedChallengeForContractsTimeoutResponseException()
        : base("Timeout submitting signed challenge (HTTP 504).")
    {
    }

    /// <summary>Initializes a new instance for a client-side timeout, preserving the underlying exception.</summary>
    /// <param name="innerException">The underlying timeout/cancellation exception.</param>
    public SubmitSignedChallengeForContractsTimeoutResponseException(Exception innerException)
        : base("Timed out submitting signed challenge before a response was received.", innerException)
    {
    }
}
