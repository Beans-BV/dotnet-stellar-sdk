using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents account authorization flags that control how an issuing account
///     manages trust and authorization for its issued assets.
/// </summary>
public sealed class Flags
{
    /// <summary>
    ///     When true, an issuer must approve holders before they can hold the issuer's asset.
    ///     This allows issuers to verify a holder's identity or other requirements
    ///     before the holder can hold the asset.
    /// </summary>
    [JsonPropertyName("auth_required")]
    public required bool AuthRequired { get; init; }

    /// <summary>
    ///     When true, an issuer can set the authorize flag of an existing trustline
    ///     to freeze or unfreeze the assets held by an asset holder.
    ///     This allows issuers to freeze assets in case of theft or regulatory requirements.
    /// </summary>
    [JsonPropertyName("auth_revocable")]
    public required bool AuthRevocable { get; init; }

    /// <summary>
    ///     When true, the authorization flags (AuthRequired and AuthRevocable) cannot be changed.
    ///     This is used to signal to potential holders that the flags will never change,
    ///     providing certainty about the asset's authorization behavior.
    /// </summary>
    [JsonPropertyName("auth_immutable")]
    public required bool AuthImmutable { get; init; }

    /// <summary>
    ///     When true, the issuer can claw back (take away) any portion of their issued asset(s)
    ///     from any asset holder. This allows issuers to recover assets in case of theft,
    ///     regulatory requirements, or other exceptional circumstances.
    /// </summary>
    [JsonPropertyName("auth_clawback_enabled")]
    public required bool AuthClawback { get; init; }
}