using System;
using System.Text.Json.Serialization;
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
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonPropertyName("ledger_close_time")]
    public DateTimeOffset LedgerCloseTime { get; init; }

    [JsonPropertyName("offer_id")]
    public string OfferId { get; init; }

    [JsonPropertyName("base_is_seller")]
    public bool BaseIsSeller { get; init; }

    [JsonPropertyName("base_account")]
    public string BaseAccount { get; init; }

    [JsonPropertyName("base_liquidity_pool_id")]
    public LiquidityPoolId BaseLiquidityPoolId { get; set; }

    [JsonPropertyName("base_offer_id")]
    public string BaseOfferId { get; init; }

    [JsonPropertyName("base_amount")]
    public string BaseAmount { get; init; }

    [JsonPropertyName("base_asset_type")]
    public string BaseAssetType { get; init; }

    [JsonPropertyName("base_asset_code")]
    public string BaseAssetCode { get; init; }

    [JsonPropertyName("base_asset_issuer")]
    public string BaseAssetIssuer { get; init; }

    [JsonPropertyName("counter_account")]
    public string CounterAccount { get; init; }

    [JsonPropertyName("counter_liquidity_pool_id")]
    public LiquidityPoolId CounterLiquidityPoolId { get; init; }

    [JsonPropertyName("counter_offer_id")]
    public string CounterOfferId { get; init; }

    [JsonPropertyName("counter_amount")]
    public string CounterAmount { get; init; }

    [JsonPropertyName("counter_asset_type")]
    public string CounterAssetType { get; init; }

    [JsonPropertyName("counter_asset_code")]
    public string CounterAssetCode { get; init; }

    [JsonPropertyName("counter_asset_issuer")]
    public string CounterAssetIssuer { get; init; }

    [JsonPropertyName("price")]
    public Price Price { get; init; }

    [JsonPropertyName("_links")]
    public TradeResponseLinks Links { get; init; }

    /// <summary>
    ///     Creates and returns a base asset.
    /// </summary>
    public Asset BaseAsset => Asset.Create(BaseAssetType, BaseAssetCode, BaseAssetIssuer);

    /// <summary>
    ///     Creates and returns a counter asset.
    /// </summary>
    public Asset CountAsset => Asset.Create(CounterAssetType, CounterAssetCode, CounterAssetIssuer);

    [JsonPropertyName("paging_token")]
    public string PagingToken { get; init; }

    public class TradeResponseLinks
    {
        [JsonPropertyName("base")]
        public Link<AssetResponse> Base { get; init; }

        [JsonPropertyName("counter")]
        public Link<AssetResponse> Counter { get; init; }

        [JsonPropertyName("operation")]
        public Link<OperationResponse> Operation { get; init; }
    }
}