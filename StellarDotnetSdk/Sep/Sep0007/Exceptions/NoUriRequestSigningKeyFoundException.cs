namespace StellarDotnetSdk.Sep.Sep0007.Exceptions;

/// <summary>Thrown when the origin domain's stellar.toml does not declare a <c>URI_REQUEST_SIGNING_KEY</c>.</summary>
public class NoUriRequestSigningKeyFoundException : Sep7Exception
{
    /// <summary>Initializes a new instance of the <see cref="NoUriRequestSigningKeyFoundException" /> class.</summary>
    /// <param name="domain">The origin domain whose stellar.toml was missing the key.</param>
    public NoUriRequestSigningKeyFoundException(string domain)
        : base($"No URI_REQUEST_SIGNING_KEY found in stellar.toml for domain '{domain}'.")
    {
        Domain = domain;
    }

    /// <summary>The origin domain whose stellar.toml was missing the key.</summary>
    public string Domain { get; }
}
