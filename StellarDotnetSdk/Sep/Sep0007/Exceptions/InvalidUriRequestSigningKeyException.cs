namespace StellarDotnetSdk.Sep.Sep0007.Exceptions;

/// <summary>
///     Thrown when the origin domain's stellar.toml declares a <c>URI_REQUEST_SIGNING_KEY</c> that is not a
///     valid Stellar account id (G...).
/// </summary>
public class InvalidUriRequestSigningKeyException : Sep7Exception
{
    /// <summary>Initializes a new instance of the <see cref="InvalidUriRequestSigningKeyException" /> class.</summary>
    /// <param name="domain">The origin domain whose stellar.toml declared the key.</param>
    /// <param name="signingKey">The invalid key value.</param>
    public InvalidUriRequestSigningKeyException(string domain, string signingKey)
        : base($"The URI_REQUEST_SIGNING_KEY {Sep7UriParser.Echo(signingKey)} in stellar.toml for domain '{domain}' is not a valid account id.")
    {
        Domain = domain;
        SigningKey = signingKey;
    }

    /// <summary>The origin domain whose stellar.toml declared the key.</summary>
    public string Domain { get; }

    /// <summary>The invalid key value.</summary>
    public string SigningKey { get; }
}
