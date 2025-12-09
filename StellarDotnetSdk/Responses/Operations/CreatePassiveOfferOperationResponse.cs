using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents a create_passive_sell_offer operation response.
///     Creates an offer to sell an asset that does not take another offer of equal price when created.
///     Unlike active offers, passive offers will not immediately match with existing offers at the same price.
/// </summary>
public class CreatePassiveOfferOperationResponse : OperationResponse
{
    public override int TypeId => 4;

    /// <summary>
    ///     The amount of the selling asset offered for sale.
    /// </summary>
    [JsonPropertyName("amount")]
    public required string Amount { get; init; }

    /// <summary>
    ///     The price ratio as a fraction representing the number of units of selling asset per unit of buying asset.
    /// </summary>
    [JsonPropertyName("price_r")]
    public required Price PriceRatio { get; init; }

    /// <summary>
    ///     The decimal representation of the price indicating how many units of selling asset equals one unit of buying asset.
    /// </summary>
    [JsonPropertyName("price")]
    public required string Price { get; init; }

    /// <summary>
    ///     The type of asset being bought (e.g., "native", "credit_alphanum4", "credit_alphanum12").
    /// </summary>
    [JsonPropertyName("buying_asset_type")]
    public required string BuyingAssetType { get; init; }

    /// <summary>
    ///     The asset code of the asset being bought (e.g., "USD", "BTC"). Only present for non-native assets.
    /// </summary>
    [JsonPropertyName("buying_asset_code")]
    public string? BuyingAssetCode { get; init; }

    /// <summary>
    ///     The account address that issued the asset being bought. Only present for non-native assets.
    /// </summary>
    [JsonPropertyName("buying_asset_issuer")]
    public string? BuyingAssetIssuer { get; init; }

    /// <summary>
    ///     The type of asset being sold (e.g., "native", "credit_alphanum4", "credit_alphanum12").
    /// </summary>
    [JsonPropertyName("selling_asset_type")]
    public required string SellingAssetType { get; init; }

    /// <summary>
    ///     The asset code of the asset being sold (e.g., "USD", "BTC"). Only present for non-native assets.
    /// </summary>
    [JsonPropertyName("selling_asset_code")]
    public string? SellingAssetCode { get; init; }

    /// <summary>
    ///     The account address that issued the asset being sold. Only present for non-native assets.
    /// </summary>
    [JsonPropertyName("selling_asset_issuer")]
    public string? SellingAssetIssuer { get; init; }

    /// <summary>
    ///     The asset being bought in this offer.
    /// </summary>
    [JsonIgnore]
    public Asset BuyingAsset => Asset.Create(BuyingAssetType, BuyingAssetCode, BuyingAssetIssuer);

    /// <summary>
    ///     The asset being sold in this offer.
    /// </summary>
    [JsonIgnore]
    public Asset SellingAsset => Asset.Create(SellingAssetType, SellingAssetCode, SellingAssetIssuer);
}