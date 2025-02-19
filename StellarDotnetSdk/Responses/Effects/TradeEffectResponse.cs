using Newtonsoft.Json;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents trade effect response.
/// </summary>
public class TradeEffectResponse : EffectResponse
{
    public override int TypeId => 33;

    [JsonProperty(PropertyName = "seller")]
    public string Seller { get; init; }

    [JsonProperty(PropertyName = "seller_muxed")]
    public string SellerMuxed { get; init; }

    [JsonProperty(PropertyName = "seller_muxed_id")]
    public long? SellerMuxedId { get; init; }

    [JsonProperty(PropertyName = "offer_id")]
    public string OfferId { get; init; }

    [JsonProperty(PropertyName = "sold_amount")]
    public string SoldAmount { get; init; }

    [JsonProperty(PropertyName = "sold_asset_type")]
    public string SoldAssetType { get; init; }

    [JsonProperty(PropertyName = "sold_asset_code")]
    public string SoldAssetCode { get; init; }

    [JsonProperty(PropertyName = "sold_asset_issuer")]
    public string SoldAssetIssuer { get; init; }

    [JsonProperty(PropertyName = "bought_amount")]
    public string BoughtAmount { get; init; }

    [JsonProperty(PropertyName = "bought_asset_type")]
    public string BoughtAssetType { get; init; }

    [JsonProperty(PropertyName = "bought_asset_code")]
    public string BoughtAssetCode { get; init; }

    [JsonProperty(PropertyName = "bought_asset_issuer")]
    public string BoughtAssetIssuer { get; init; }

    public Asset BoughtAsset => Asset.Create(BoughtAssetType, BoughtAssetCode, BoughtAssetIssuer);

    public Asset SoldAsset => Asset.Create(SoldAssetType, SoldAssetCode, SoldAssetIssuer);
}