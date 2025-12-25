namespace StellarDotnetSdk.Sep.Sep0001;

/// <summary>
///     Point of Contact Documentation. From the stellar.toml PRINCIPALS list.
///     It contains identifying information for the primary point of contact or principal of the organization.
///     See <a href="https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0001.md">SEP-0001</a>
/// </summary>
public sealed record PointOfContact
{
    /// <summary>
    ///     Full legal name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    ///     Business email address for the principal.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    ///     Personal Keybase account. Should include proof of ownership for other online accounts, as well as the
    ///     organization's domain.
    /// </summary>
    public string? Keybase { get; init; }

    /// <summary>
    ///     Personal Telegram account.
    /// </summary>
    public string? Telegram { get; init; }

    /// <summary>
    ///     Personal Twitter account.
    /// </summary>
    public string? Twitter { get; init; }

    /// <summary>
    ///     Personal Github account.
    /// </summary>
    public string? Github { get; init; }

    /// <summary>
    ///     SHA-256 hash of a photo of the principal's government-issued photo ID.
    /// </summary>
    public string? IdPhotoHash { get; init; }

    /// <summary>
    ///     SHA-256 hash of a verification photo of principal. Should be well-lit and contain: principal holding ID card and
    ///     signed,
    ///     dated, hand-written message stating I, $name, am a principal of $orgName, a Stellar token issuer with address
    ///     $issuerAddress.
    /// </summary>
    public string? VerificationPhotoHash { get; init; }
}