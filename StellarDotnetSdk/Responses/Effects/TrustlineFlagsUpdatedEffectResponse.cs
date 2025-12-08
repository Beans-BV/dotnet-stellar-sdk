using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the trustline_flags_updated effect response.
///     This effect occurs when trustline flags are updated.
/// </summary>
public sealed class TrustlineFlagsUpdatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 26;

    /// <summary>
    ///     The type of the trusted asset.
    /// </summary>
    [JsonPropertyName("asset_type")]
    public required string AssetType { get; init; }

    /// <summary>
    ///     The code of the trusted asset.
    /// </summary>
    [JsonPropertyName("asset_code")]
    public string? AssetCode { get; init; }

    /// <summary>
    ///     The issuer of the trusted asset.
    /// </summary>
    [JsonPropertyName("asset_issuer")]
    public string? AssetIssuer { get; init; }

    /// <summary>
    ///     The account ID of the trustor whose trustline was modified.
    /// </summary>
    [JsonPropertyName("trustor")]
    public required string Trustor { get; init; }

    /// <summary>
    ///     Whether the trustline is fully authorized.
    /// </summary>
    [JsonPropertyName("authorized_flag")]
    public bool AuthorizedFlag { get; init; }

    /// <summary>
    ///     Whether the trustline is authorized to maintain liabilities only.
    /// </summary>
    [JsonPropertyName("authorized_to_maintain_liabilities")]
    public bool AuthorizedToMaintainLiabilities { get; init; }

    /// <summary>
    ///     Whether the clawback feature is enabled for this trustline.
    /// </summary>
    [JsonPropertyName("clawback_enabled_flag")]
    public bool ClawbackEnabledFlag { get; init; }
}