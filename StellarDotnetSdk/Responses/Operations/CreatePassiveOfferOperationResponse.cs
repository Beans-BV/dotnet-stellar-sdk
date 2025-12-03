using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <summary>
///     Represents CreatePassiveOffer operation response.
/// </summary>
public class CreatePassiveOfferOperationResponse : OperationResponse
{
    public override int TypeId => 4;

    [JsonPropertyName("offer_id")]
    public int OfferId { get; init; }

    [JsonPropertyName("amount")]
    public string Amount { get; init; }

    /// <summary>
    ///     The ask price as a ratio.
    /// </summary>
    [JsonPropertyName("price_r")]
    public Responses.Price PriceRatio { get; init; }

    /// <summary>
    ///     The ask price.
    /// </summary>
    [JsonPropertyName("price")]
    public string Price { get; init; }

    [JsonPropertyName("buying_asset_type")]
    public string BuyingAssetType { get; init; }

    [JsonPropertyName("buying_asset_code")]
    public string BuyingAssetCode { get; init; }

    [JsonPropertyName("buying_asset_issuer")]
    public string BuyingAssetIssuer { get; init; }

    [JsonPropertyName("selling_asset_type")]
    public string SellingAssetType { get; init; }

    [JsonPropertyName("selling_asset_code")]
    public string SellingAssetCode { get; init; }

    [JsonPropertyName("selling_asset_issuer")]
    public string SellingAssetIssuer { get; init; }

    public Asset BuyingAsset => Asset.Create(BuyingAssetType, BuyingAssetCode, BuyingAssetIssuer);

    public Asset SellingAsset => Asset.Create(SellingAssetType, SellingAssetCode, SellingAssetIssuer);
}