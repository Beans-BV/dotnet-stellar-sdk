namespace StellarDotnetSdk.Sep.Sep0007.Exceptions;

/// <summary>
///     Thrown when the origin domain's current <c>URI_REQUEST_SIGNING_KEY</c> differs from the key the wallet
///     pinned for that domain earlier. SEP-7 requires wallets to alert the user when the key changes, because
///     it may mean the domain was compromised. The signature itself is not checked when this is thrown.
/// </summary>
public class UriRequestSigningKeyChangedException : Sep7Exception
{
    /// <summary>Initializes a new instance of the <see cref="UriRequestSigningKeyChangedException" /> class.</summary>
    /// <param name="domain">The origin domain.</param>
    /// <param name="pinnedSigningKey">The key the wallet cached for the domain earlier.</param>
    /// <param name="currentSigningKey">The key the domain's stellar.toml declares now.</param>
    public UriRequestSigningKeyChangedException(string domain, string pinnedSigningKey, string currentSigningKey)
        : base($"The URI_REQUEST_SIGNING_KEY for domain '{domain}' changed from '{pinnedSigningKey}' to '{currentSigningKey}'.")
    {
        Domain = domain;
        PinnedSigningKey = pinnedSigningKey;
        CurrentSigningKey = currentSigningKey;
    }

    /// <summary>The origin domain.</summary>
    public string Domain { get; }

    /// <summary>The key the wallet cached for the domain earlier.</summary>
    public string PinnedSigningKey { get; }

    /// <summary>The key the domain's stellar.toml declares now.</summary>
    public string CurrentSigningKey { get; }
}
