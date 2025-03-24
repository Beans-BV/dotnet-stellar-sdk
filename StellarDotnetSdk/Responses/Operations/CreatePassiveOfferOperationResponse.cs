using Newtonsoft.Json;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <summary>
///     Represents CreatePassiveOffer operation response.
/// </summary>
public class CreatePassiveOfferOperationResponse : OperationResponse
{
    public override int TypeId => 4;

    [JsonProperty(PropertyName = "offer_id")]
    public int OfferId { get; init; }

    [JsonProperty(PropertyName = "amount")]
    public string Amount { get; init; }

    /// <summary>
    ///     The ask price as a ratio.
    /// </summary>
    [JsonProperty(PropertyName = "price_r")]
    public StellarDotnetSdk.Price PriceRatio { get; init; }

    /// <summary>
    ///     The ask price.
    /// </summary>
    [JsonProperty(PropertyName = "price")]
    public string Price { get; init; }

    [JsonProperty(PropertyName = "buying_asset_type")]
    public string BuyingAssetType { get; init; }

    [JsonProperty(PropertyName = "buying_asset_code")]
    public string BuyingAssetCode { get; init; }

    [JsonProperty(PropertyName = "buying_asset_issuer")]
    public string BuyingAssetIssuer { get; init; }

    [JsonProperty(PropertyName = "selling_asset_type")]
    public string SellingAssetType { get; init; }

    [JsonProperty(PropertyName = "selling_asset_code")]
    public string SellingAssetCode { get; init; }

    [JsonProperty(PropertyName = "selling_asset_issuer")]
    public string SellingAssetIssuer { get; init; }

    public Asset BuyingAsset => Asset.Create(BuyingAssetType, BuyingAssetCode, BuyingAssetIssuer);

    public Asset SellingAsset => Asset.Create(SellingAssetType, SellingAssetCode, SellingAssetIssuer);
}