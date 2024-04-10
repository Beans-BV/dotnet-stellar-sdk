using Newtonsoft.Json;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <summary>
///     Represents ChangeTrust operation response.
/// </summary>
public class ChangeTrustOperationResponse : OperationResponse
{
    public override int TypeId => 6;

    [JsonProperty(PropertyName = "asset_code")]
    public string AssetCode { get; init; }

    [JsonProperty(PropertyName = "asset_issuer")]
    public string AssetIssuer { get; init; }

    [JsonProperty(PropertyName = "asset_type")]
    public string AssetType { get; init; }

    [JsonProperty(PropertyName = "limit")] public string Limit { get; init; }

    [JsonProperty(PropertyName = "trustee")]
    public string Trustee { get; init; }

    [JsonProperty(PropertyName = "trustor")]
    public string Trustor { get; init; }

    [JsonProperty(PropertyName = "trustor_muxed")]
    public string TrustorMuxed { get; init; }

    [JsonProperty(PropertyName = "trustor_muxed_id")]
    public ulong? TrustorMuxedID { get; init; }

    public AssetTypeCreditAlphaNum Asset => Assets.Asset.CreateNonNativeAsset(AssetCode, AssetIssuer);
}