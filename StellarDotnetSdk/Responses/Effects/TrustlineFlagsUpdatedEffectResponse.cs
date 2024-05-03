using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents trustline_updated effect response.
/// </summary>
public class TrustlineFlagsUpdatedEffectResponse : EffectResponse
{
    public override int TypeId => 26;

    [JsonProperty(PropertyName = "asset_type")]
    public string AssetType { get; init; }

    [JsonProperty(PropertyName = "asset_code")]
    public string AssetCode { get; init; }

    [JsonProperty(PropertyName = "asset_issuer")]
    public string AssetIssuer { get; init; }

    [JsonProperty(PropertyName = "trustor")]
    public string Trustor { get; init; }

    [JsonProperty(PropertyName = "authorized_flag")]
    public bool AuthorizedFlag { get; init; }

    [JsonProperty(PropertyName = "authorized_to_maintain_liabilities")]
    public bool AuthorizedToMaintainLiabilities { get; init; }

    [JsonProperty(PropertyName = "clawback_enabled_flag")]
    public bool ClawbackEnabledFlag { get; init; }
}