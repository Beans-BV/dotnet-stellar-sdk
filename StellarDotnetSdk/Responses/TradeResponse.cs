using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents a trade that was executed on the Stellar decentralized exchange.
///     A trade occurs when offers are matched or when interacting with a liquidity pool.
/// </summary>
public sealed class TradeResponse : Response, IPagingToken
{
    /// <summary>
    ///     The unique identifier for this trade.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    ///     An ISO 8601 formatted string of when the ledger containing this trade closed.
    /// </summary>
    [JsonPropertyName("ledger_close_time")]
    public DateTimeOffset LedgerCloseTime { get; init; }

    /// <summary>
    ///     Type of this trade: liquidity_pool or orderbook
    /// </summary>
    [JsonPropertyName("trade_type")]
    public required string TradeType { get; init; }

    /// <summary>
    ///     The fee charged for trades in this pool, expressed in basis points (1/100 of a percent).
    /// </summary>
    [JsonPropertyName("liquidity_pool_fee_bp")]
    public uint? LiquidityPoolFeeBp { get; init; }

    /// <summary>
    ///     Whether the base party was the seller in this trade.
    /// </summary>
    [JsonPropertyName("base_is_seller")]
    public required bool BaseIsSeller { get; init; }

    /// <summary>
    ///     The account ID of the base party for this trade.
    /// </summary>
    [JsonPropertyName("base_account")]
    public string? BaseAccount { get; init; }

    /// <summary>
    ///     The base liquidity pool ID. If this trade was executed against a liquidity pool.
    /// </summary>
    [JsonPropertyName("base_liquidity_pool_id")]
    public LiquidityPoolId? BaseLiquidityPoolId { get; set; }

    /// <summary>
    ///     The offer ID of the base party's offer that was involved in the trade.
    /// </summary>
    [JsonPropertyName("base_offer_id")]
    public string? BaseOfferId { get; init; }

    /// <summary>
    ///     The amount of the base asset that was traded.
    ///     Represented as a string to preserve precision.
    /// </summary>
    [JsonPropertyName("base_amount")]
    public required string BaseAmount { get; init; }

    /// <summary>
    ///     The type of the base asset: "native", "credit_alphanum4", or "credit_alphanum12".
    /// </summary>
    [JsonPropertyName("base_asset_type")]
    public required string BaseAssetType { get; init; }

    /// <summary>
    ///     The code of the base asset. Null for native XLM.
    /// </summary>
    [JsonPropertyName("base_asset_code")]
    public string? BaseAssetCode { get; init; }

    /// <summary>
    ///     The issuer of the base asset. Null for native XLM.
    /// </summary>
    [JsonPropertyName("base_asset_issuer")]
    public string? BaseAssetIssuer { get; init; }

    /// <summary>
    ///     The account ID of the counter party in this trade (if not a liquidity pool).
    /// </summary>
    [JsonPropertyName("counter_account")]
    public string? CounterAccount { get; init; }

    /// <summary>
    ///     The liquidity pool ID if the counter party is a liquidity pool.
    /// </summary>
    [JsonPropertyName("counter_liquidity_pool_id")]
    public LiquidityPoolId? CounterLiquidityPoolId { get; init; }

    /// <summary>
    ///     The offer ID of the counter party's offer that was involved in the trade.
    /// </summary>
    [JsonPropertyName("counter_offer_id")]
    public string? CounterOfferId { get; init; }

    /// <summary>
    ///     The amount of the counter asset that was traded.
    ///     Represented as a string to preserve precision.
    /// </summary>
    [JsonPropertyName("counter_amount")]
    public required string CounterAmount { get; init; }

    /// <summary>
    ///     The type of the counter asset: "native", "credit_alphanum4", or "credit_alphanum12".
    /// </summary>
    [JsonPropertyName("counter_asset_type")]
    public required string CounterAssetType { get; init; }

    /// <summary>
    ///     The code of the counter asset. Null for native XLM.
    /// </summary>
    [JsonPropertyName("counter_asset_code")]
    public string? CounterAssetCode { get; init; }

    /// <summary>
    ///     The issuer of the counter asset. Null for native XLM.
    /// </summary>
    [JsonPropertyName("counter_asset_issuer")]
    public string? CounterAssetIssuer { get; init; }

    /// <summary>
    ///     The price of the trade (counter asset / base asset).
    /// </summary>
    [JsonPropertyName("price")]
    public required Price Price { get; init; }

    /// <summary>
    ///     Links to related resources for this trade.
    /// </summary>
    [JsonPropertyName("_links")]
    public required TradeResponseLinks Links { get; init; }

    /// <summary>
    ///     Creates and returns the base asset object.
    /// </summary>
    public Asset BaseAsset => Asset.Create(BaseAssetType, BaseAssetCode, BaseAssetIssuer);

    /// <summary>
    ///     Creates and returns the counter asset object.
    /// </summary>
    public Asset CounterAsset => Asset.Create(CounterAssetType, CounterAssetCode, CounterAssetIssuer);

    /// <inheritdoc />
    [JsonPropertyName("paging_token")]
    public required string PagingToken { get; init; }

    /// <summary>
    ///     Links to related resources for a trade.
    /// </summary>
    public sealed class TradeResponseLinks
    {
        /// <summary>
        ///     Link to the base account information.
        /// </summary>
        /// <remarks>
        ///     This can either be Link&lt;LiquidityPoolResponse&gt; or Link&lt;AccountResponse&gt; depending on the trade
        ///     type.
        /// </remarks>
        [JsonPropertyName("base")]
        public required Link Base { get; init; }

        /// <summary>
        ///     Link to the counter account information.
        /// </summary>
        /// <remarks>
        ///     This can either be Link&lt;LiquidityPoolResponse&gt; or Link&lt;AccountResponse&gt; depending on the trade
        ///     type.
        /// </remarks>
        [JsonPropertyName("counter")]
        public required Link Counter { get; init; }

        /// <summary>
        ///     Link to the operation that caused this trade.
        /// </summary>
        [JsonPropertyName("operation")]
        public required Link<OperationResponse> Operation { get; init; }
    }
}