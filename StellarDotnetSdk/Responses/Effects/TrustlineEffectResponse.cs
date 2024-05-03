using Newtonsoft.Json;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

public abstract class TrustlineEffectResponse : EffectResponse
{
    [JsonProperty(PropertyName = "limit")] public string Limit { get; init; }

    [JsonProperty(PropertyName = "asset_type")]
    public string AssetType { get; init; }

    [JsonProperty(PropertyName = "asset_code")]
    public string AssetCode { get; init; }

    [JsonProperty(PropertyName = "asset_issuer")]
    public string AssetIssuer { get; init; }

    public AssetTypeCreditAlphaNum Asset => Assets.Asset.CreateNonNativeAsset(AssetCode, AssetIssuer);
}

/// <summary>
///     Represents trustline_created effect response.
/// </summary>
public class TrustlineCreatedEffectResponse : TrustlineEffectResponse
{
    public override int TypeId => 20;
}

/// <summary>
///     Represents trustline_removed effect response.
/// </summary>
public class TrustlineRemovedEffectResponse : TrustlineEffectResponse
{
    public override int TypeId => 21;
}

/// <summary>
///     Represents trustline_updated effect response.
/// </summary>
public class TrustlineUpdatedEffectResponse : TrustlineEffectResponse
{
    public override int TypeId => 22;
}