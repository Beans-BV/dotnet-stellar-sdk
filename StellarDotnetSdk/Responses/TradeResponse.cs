using Newtonsoft.Json;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses;
#nullable disable

/// <summary>
///     Represents trades response.
/// </summary>
public class TradeResponse : Response, IPagingToken
{
    [JsonProperty(PropertyName = "id")] public string Id { get; init; }

    [JsonProperty(PropertyName = "ledger_close_time")]
    public string LedgerCloseTime { get; init; }

    [JsonProperty(PropertyName = "offer_id")]
    public string OfferId { get; init; }

    [JsonProperty(PropertyName = "base_is_seller")]
    public bool BaseIsSeller { get; init; }

    [JsonProperty(PropertyName = "base_account")]
    public string BaseAccount { get; init; }

    [JsonProperty(PropertyName = "base_liquidity_pool_id")]
    public LiquidityPoolId BaseLiquidityPoolId { get; set; }

    [JsonProperty(PropertyName = "base_offer_id")]
    public string BaseOfferId { get; init; }

    [JsonProperty(PropertyName = "base_amount")]
    public string BaseAmount { get; init; }

    [JsonProperty(PropertyName = "base_asset_type")]
    public string BaseAssetType { get; init; }

    [JsonProperty(PropertyName = "base_asset_code")]
    public string BaseAssetCode { get; init; }

    [JsonProperty(PropertyName = "base_asset_issuer")]
    public string BaseAssetIssuer { get; init; }

    [JsonProperty(PropertyName = "counter_account")]
    public string CounterAccount { get; init; }

    [JsonProperty(PropertyName = "counter_liquidity_pool_id")]
    public LiquidityPoolId CounterLiquidityPoolId { get; init; }

    [JsonProperty(PropertyName = "counter_offer_id")]
    public string CounterOfferId { get; init; }

    [JsonProperty(PropertyName = "counter_amount")]
    public string CounterAmount { get; init; }

    [JsonProperty(PropertyName = "counter_asset_type")]
    public string CounterAssetType { get; init; }

    [JsonProperty(PropertyName = "counter_asset_code")]
    public string CounterAssetCode { get; init; }

    [JsonProperty(PropertyName = "counter_asset_issuer")]
    public string CounterAssetIssuer { get; init; }

    [JsonProperty(PropertyName = "price")] public Price Price { get; init; }

    [JsonProperty(PropertyName = "_links")]
    public TradeResponseLinks Links { get; init; }

    /// <summary>
    ///     Creates and returns a base asset.
    /// </summary>
    public Asset BaseAsset => Asset.Create(BaseAssetType, BaseAssetCode, BaseAssetIssuer);

    /// <summary>
    ///     Creates and returns a counter asset.
    /// </summary>
    public Asset CountAsset => Asset.Create(CounterAssetType, CounterAssetCode, CounterAssetIssuer);

    [JsonProperty(PropertyName = "paging_token")]
    public string PagingToken { get; init; }

    public class TradeResponseLinks
    {
        [JsonProperty(PropertyName = "base")] public Link<AssetResponse> Base { get; init; }

        [JsonProperty(PropertyName = "counter")]
        public Link<AssetResponse> Counter { get; init; }

        [JsonProperty(PropertyName = "operation")]
        public Link<OperationResponse> Operation { get; init; }
    }
}