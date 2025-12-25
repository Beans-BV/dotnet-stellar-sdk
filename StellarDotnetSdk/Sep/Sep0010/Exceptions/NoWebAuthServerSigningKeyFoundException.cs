namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Exception thrown when SIGNING_KEY is not found in the domain's stellar.toml file.
/// </summary>
public class NoWebAuthServerSigningKeyFoundException : WebAuthException
{
    public NoWebAuthServerSigningKeyFoundException(string domain)
        : base($"No auth server SIGNING_KEY found in stellar.toml for domain: {domain}")
    {
        Domain = domain;
    }

    /// <summary>
    ///     The domain where the auth server SIGNING_KEY was not found.
    /// </summary>
    public string Domain { get; }
}