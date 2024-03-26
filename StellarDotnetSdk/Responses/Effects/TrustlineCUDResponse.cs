using Newtonsoft.Json;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;

public abstract class TrustlineCUDResponse : EffectResponse
{
    public TrustlineCUDResponse()
    {
    }

    protected TrustlineCUDResponse(string limit, string assetType, string assetCode, string assetIssuer)
    {
        Limit = limit;
        AssetType = assetType;
        AssetCode = assetCode;
        AssetIssuer = assetIssuer;
    }

    [JsonProperty(PropertyName = "limit")] public string Limit { get; private set; }

    [JsonProperty(PropertyName = "asset_type")]
    public string AssetType { get; private set; }

    [JsonProperty(PropertyName = "asset_code")]
    public string AssetCode { get; private set; }

    [JsonProperty(PropertyName = "asset_issuer")]
    public string AssetIssuer { get; private set; }

    public AssetTypeCreditAlphaNum Asset => Assets.Asset.CreateNonNativeAsset(AssetCode, AssetIssuer);
}