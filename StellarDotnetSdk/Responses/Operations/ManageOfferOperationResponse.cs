using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents a manage_offer operation response.
///     Creates, updates, or deletes an offer to buy or sell assets on the Stellar decentralized exchange.
///     This is the base class for both manage_sell_offer and manage_buy_offer operations.
/// </summary>
public abstract class ManageOfferOperationResponse : OperationResponse
{
    /// <summary>
    ///     The unique identifier for the offer. A value of "0" indicates the offer was deleted.
    /// </summary>
    [JsonPropertyName("offer_id")]
    public required string OfferId { get; init; }

    /// <summary>
    ///     The amount of the selling asset to be sold (for manage_sell_offer) or the amount of buying asset to be bought (for
    ///     manage_buy_offer).
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

/// <summary>
///     Represents a manage_buy_offer operation response.
///     Creates, updates, or deletes an offer to buy a specific amount of an asset at a given price.
/// </summary>
public class ManageBuyOfferOperationResponse : ManageOfferOperationResponse
{
    public override int TypeId => 12;
}

/// <summary>
///     Represents a manage_sell_offer operation response.
///     Creates, updates, or deletes an offer to sell a specific amount of an asset at a given price.
/// </summary>
public class ManageSellOfferOperationResponse : ManageOfferOperationResponse
{
    public override int TypeId => 3;
}