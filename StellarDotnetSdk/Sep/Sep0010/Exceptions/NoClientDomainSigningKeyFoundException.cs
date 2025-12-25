namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Exception thrown when client domain's SIGNING_KEY is not found in stellar.toml.
/// </summary>
public class NoClientDomainSigningKeyFoundException : WebAuthException
{
    public NoClientDomainSigningKeyFoundException(string domain)
        : base($"No client domain SIGNING_KEY found in stellar.toml for domain: {domain}")
    {
        Domain = domain;
    }

    /// <summary>
    ///     The client domain where SIGNING_KEY was not found.
    /// </summary>
    public string Domain { get; }
}