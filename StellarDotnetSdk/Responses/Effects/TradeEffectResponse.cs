using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents trade effect response.
/// </summary>
public class TradeEffectResponse : EffectResponse
{
    public override int TypeId => 33;

    [JsonPropertyName("seller")]
    public string Seller { get; init; }

    [JsonPropertyName("seller_muxed")]
    public string SellerMuxed { get; init; }

    [JsonPropertyName("seller_muxed_id")]
    public long? SellerMuxedId { get; init; }

    [JsonPropertyName("offer_id")]
    public string OfferId { get; init; }

    [JsonPropertyName("sold_amount")]
    public string SoldAmount { get; init; }

    [JsonPropertyName("sold_asset_type")]
    public string SoldAssetType { get; init; }

    [JsonPropertyName("sold_asset_code")]
    public string SoldAssetCode { get; init; }

    [JsonPropertyName("sold_asset_issuer")]
    public string SoldAssetIssuer { get; init; }

    [JsonPropertyName("bought_amount")]
    public string BoughtAmount { get; init; }

    [JsonPropertyName("bought_asset_type")]
    public string BoughtAssetType { get; init; }

    [JsonPropertyName("bought_asset_code")]
    public string BoughtAssetCode { get; init; }

    [JsonPropertyName("bought_asset_issuer")]
    public string BoughtAssetIssuer { get; init; }

    public Asset BoughtAsset => Asset.Create(BoughtAssetType, BoughtAssetCode, BoughtAssetIssuer);

    public Asset SoldAsset => Asset.Create(SoldAssetType, SoldAssetCode, SoldAssetIssuer);
}