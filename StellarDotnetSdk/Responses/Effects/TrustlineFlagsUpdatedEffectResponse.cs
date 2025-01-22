using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents trustline_updated effect response.
/// </summary>
public class TrustlineFlagsUpdatedEffectResponse : EffectResponse
{
    public override int TypeId => 26;

    [JsonPropertyName("asset_type")]
    public string AssetType { get; init; }

    [JsonPropertyName("asset_code")]
    public string AssetCode { get; init; }

    [JsonPropertyName("asset_issuer")]
    public string AssetIssuer { get; init; }

    [JsonPropertyName("trustor")]
    public string Trustor { get; init; }

    [JsonPropertyName("authorized_flag")]
    public bool AuthorizedFlag { get; init; }

    [JsonPropertyName("authorized_to_maintain_liabilities")]
    public bool AuthorizedToMaintainLiabilities { get; init; }

    [JsonPropertyName("clawback_enabled_flag")]
    public bool ClawbackEnabledFlag { get; init; }
}