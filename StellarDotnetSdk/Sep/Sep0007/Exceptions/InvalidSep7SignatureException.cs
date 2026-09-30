namespace StellarDotnetSdk.Sep.Sep0007.Exceptions;

/// <summary>
///     Thrown when the URI's <c>signature</c> does not verify against the origin domain's
///     <c>URI_REQUEST_SIGNING_KEY</c>. SEP-7 requires wallets to alert the user in that case.
/// </summary>
public class InvalidSep7SignatureException : Sep7Exception
{
    /// <summary>Initializes a new instance of the <see cref="InvalidSep7SignatureException" /> class.</summary>
    /// <param name="domain">The origin domain.</param>
    /// <param name="signingKey">The <c>URI_REQUEST_SIGNING_KEY</c> the signature was checked against.</param>
    public InvalidSep7SignatureException(string domain, string signingKey)
        : base($"The URI signature does not verify against the URI_REQUEST_SIGNING_KEY '{signingKey}' of domain '{domain}'.")
    {
        Domain = domain;
        SigningKey = signingKey;
    }

    /// <summary>The origin domain.</summary>
    public string Domain { get; }

    /// <summary>The <c>URI_REQUEST_SIGNING_KEY</c> the signature was checked against.</summary>
    public string SigningKey { get; }
}
