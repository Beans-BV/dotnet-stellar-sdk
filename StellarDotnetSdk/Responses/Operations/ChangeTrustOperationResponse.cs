using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <summary>
///     Represents ChangeTrust operation response.
/// </summary>
public class ChangeTrustOperationResponse : OperationResponse
{
    public override int TypeId => 6;

    [JsonPropertyName("asset_code")]
    public string AssetCode { get; init; }

    [JsonPropertyName("asset_issuer")]
    public string AssetIssuer { get; init; }

    [JsonPropertyName("asset_type")]
    public string AssetType { get; init; }

    [JsonPropertyName("limit")] public string Limit { get; init; }

    [JsonPropertyName("trustee")]
    public string Trustee { get; init; }

    [JsonPropertyName("trustor")]
    public string Trustor { get; init; }

    [JsonPropertyName("trustor_muxed")]
    public string TrustorMuxed { get; init; }

    [JsonPropertyName("trustor_muxed_id")]
    public string? TrustorMuxedID { get; init; }

    public AssetTypeCreditAlphaNum Asset => Assets.Asset.CreateNonNativeAsset(AssetCode, AssetIssuer);
}