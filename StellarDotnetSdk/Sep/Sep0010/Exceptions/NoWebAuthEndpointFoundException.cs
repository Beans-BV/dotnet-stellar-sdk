namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Exception thrown when WEB_AUTH_ENDPOINT is not found in the domain's stellar.toml file.
/// </summary>
public class NoWebAuthEndpointFoundException : WebAuthException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NoWebAuthEndpointFoundException" /> class.
    /// </summary>
    /// <param name="domain">The domain where WEB_AUTH_ENDPOINT was not found in stellar.toml.</param>
    public NoWebAuthEndpointFoundException(string domain)
        : base($"No WEB_AUTH_ENDPOINT found in stellar.toml for domain: {domain}")
    {
        Domain = domain;
    }

    /// <summary>
    ///     The domain where WEB_AUTH_ENDPOINT was not found.
    /// </summary>
    public string Domain { get; }
}