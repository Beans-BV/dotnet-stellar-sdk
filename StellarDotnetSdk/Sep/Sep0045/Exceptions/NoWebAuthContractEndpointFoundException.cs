namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when a domain's stellar.toml does not declare a WEB_AUTH_FOR_CONTRACTS_ENDPOINT.</summary>
public class NoWebAuthContractEndpointFoundException : WebAuthContractException
{
    /// <summary>Initializes a new instance of the <see cref="NoWebAuthContractEndpointFoundException" /> class.</summary>
    /// <param name="domain">The domain whose stellar.toml was missing the endpoint.</param>
    public NoWebAuthContractEndpointFoundException(string domain)
        : base($"No WEB_AUTH_FOR_CONTRACTS_ENDPOINT found in stellar.toml for domain '{domain}'.")
    {
        Domain = domain;
    }

    /// <summary>The domain whose stellar.toml was missing the endpoint.</summary>
    public string Domain { get; }
}
