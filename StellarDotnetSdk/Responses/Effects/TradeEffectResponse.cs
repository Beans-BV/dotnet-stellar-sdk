using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the trade effect response.
///     This effect occurs when a trade is executed on the DEX.
/// </summary>
public sealed class TradeEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 33;

    /// <summary>
    ///     The account ID of the seller.
    /// </summary>
    [JsonPropertyName("seller")]
    public string? Seller { get; init; }

    /// <summary>
    ///     The muxed account representation of the seller, if applicable.
    /// </summary>
    [JsonPropertyName("seller_muxed")]
    public string? SellerMuxed { get; init; }

    /// <summary>
    ///     The muxed account ID of the seller, if applicable.
    /// </summary>
    [JsonPropertyName("seller_muxed_id")]
    public ulong? SellerMuxedId { get; init; }

    /// <summary>
    ///     The offer ID that was matched.
    /// </summary>
    [JsonPropertyName("offer_id")]
    public string? OfferId { get; init; }

    /// <summary>
    ///     The amount of the sold asset.
    /// </summary>
    [JsonPropertyName("sold_amount")]
    public string? SoldAmount { get; init; }

    /// <summary>
    ///     The type of the sold asset: "native", "credit_alphanum4", or "credit_alphanum12".
    /// </summary>
    [JsonPropertyName("sold_asset_type")]
    public string? SoldAssetType { get; init; }

    /// <summary>
    ///     The code of the sold asset. Null for native XLM.
    /// </summary>
    [JsonPropertyName("sold_asset_code")]
    public string? SoldAssetCode { get; init; }

    /// <summary>
    ///     The issuer of the sold asset. Null for native XLM.
    /// </summary>
    [JsonPropertyName("sold_asset_issuer")]
    public string? SoldAssetIssuer { get; init; }

    /// <summary>
    ///     The amount of the bought asset.
    /// </summary>
    [JsonPropertyName("bought_amount")]
    public string? BoughtAmount { get; init; }

    /// <summary>
    ///     The type of the bought asset: "native", "credit_alphanum4", or "credit_alphanum12".
    /// </summary>
    [JsonPropertyName("bought_asset_type")]
    public string? BoughtAssetType { get; init; }

    /// <summary>
    ///     The code of the bought asset. Null for native XLM.
    /// </summary>
    [JsonPropertyName("bought_asset_code")]
    public string? BoughtAssetCode { get; init; }

    /// <summary>
    ///     The issuer of the bought asset. Null for native XLM.
    /// </summary>
    [JsonPropertyName("bought_asset_issuer")]
    public string? BoughtAssetIssuer { get; init; }

    /// <summary>
    ///     The bought asset.
    /// </summary>
    public Asset? BoughtAsset => BoughtAssetType != null
        ? Asset.Create(BoughtAssetType, BoughtAssetCode, BoughtAssetIssuer)
        : null;

    /// <summary>
    ///     The sold asset.
    /// </summary>
    public Asset? SoldAsset => SoldAssetType != null
        ? Asset.Create(SoldAssetType, SoldAssetCode, SoldAssetIssuer)
        : null;
}