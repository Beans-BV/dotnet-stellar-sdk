using Newtonsoft.Json;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents ChangeTrust operation response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/operation.html
///     <seealso cref="Requests.OperationsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class ChangeTrustOperationResponse : OperationResponse
{
    public ChangeTrustOperationResponse()
    {
    }

    public ChangeTrustOperationResponse(string assetCode, string assetIssuer, string assetType, string limit,
        string trustee, string trustor)
    {
        AssetCode = assetCode;
        AssetIssuer = assetIssuer;
        AssetType = assetType;
        Limit = limit;
        Trustee = trustee;
        Trustor = trustor;
    }

    public override int TypeId => 6;

    [JsonProperty(PropertyName = "asset_code")]
    public string AssetCode { get; private set; }

    [JsonProperty(PropertyName = "asset_issuer")]
    public string AssetIssuer { get; private set; }

    [JsonProperty(PropertyName = "asset_type")]
    public string AssetType { get; private set; }

    [JsonProperty(PropertyName = "limit")] public string Limit { get; private set; }

    [JsonProperty(PropertyName = "trustee")]
    public string Trustee { get; private set; }

    [JsonProperty(PropertyName = "trustor")]
    public string Trustor { get; private set; }

    [JsonProperty(PropertyName = "trustor_muxed")]
    public string TrustorMuxed { get; private set; }

    [JsonProperty(PropertyName = "trustor_muxed_id")]
    public ulong? TrustorMuxedID { get; private set; }

    public AssetTypeCreditAlphaNum Asset => Assets.Asset.CreateNonNativeAsset(AssetCode, AssetIssuer);
}