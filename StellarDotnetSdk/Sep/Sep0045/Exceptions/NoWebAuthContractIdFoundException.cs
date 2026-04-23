namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when a domain's stellar.toml does not declare a WEB_AUTH_CONTRACT_ID.</summary>
public class NoWebAuthContractIdFoundException : WebAuthContractException
{
    /// <summary>Initializes a new instance of the <see cref="NoWebAuthContractIdFoundException" /> class.</summary>
    /// <param name="domain">The domain whose stellar.toml was missing the contract id.</param>
    public NoWebAuthContractIdFoundException(string domain)
        : base($"No WEB_AUTH_CONTRACT_ID found in stellar.toml for domain '{domain}'.")
    {
        Domain = domain;
    }

    /// <summary>The domain whose stellar.toml was missing the contract id.</summary>
    public string Domain { get; }
}
