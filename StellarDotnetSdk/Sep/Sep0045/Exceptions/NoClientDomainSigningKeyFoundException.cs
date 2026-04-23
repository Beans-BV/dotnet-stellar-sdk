namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when the client domain's stellar.toml does not declare a SIGNING_KEY.</summary>
public class NoClientDomainSigningKeyFoundException : WebAuthContractException
{
    /// <summary>Initializes a new instance of the <see cref="NoClientDomainSigningKeyFoundException" /> class.</summary>
    /// <param name="domain">The client domain whose stellar.toml was missing the signing key.</param>
    public NoClientDomainSigningKeyFoundException(string domain)
        : base($"No SIGNING_KEY found in stellar.toml for client domain '{domain}'.")
    {
        Domain = domain;
    }

    /// <summary>The client domain whose stellar.toml was missing the signing key.</summary>
    public string Domain { get; }
}
