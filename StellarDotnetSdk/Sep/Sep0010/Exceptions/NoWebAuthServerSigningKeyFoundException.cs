namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Exception thrown when SIGNING_KEY is not found in the domain's stellar.toml file.
/// </summary>
public class NoWebAuthServerSigningKeyFoundException : WebAuthException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NoWebAuthServerSigningKeyFoundException" /> class.
    /// </summary>
    /// <param name="domain">The domain where the auth server SIGNING_KEY was not found in stellar.toml.</param>
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