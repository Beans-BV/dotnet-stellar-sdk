using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Responses.Operations;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents a liquidity pool on the Stellar network.
///     Liquidity pools enable automated market making (AMM) for trading asset pairs.
/// </summary>
public sealed class LiquidityPoolResponse : Response, IPagingToken
{
    /// <summary>
    ///     The unique identifier for this liquidity pool.
    /// </summary>
    [JsonPropertyName("id")]
    public required LiquidityPoolId Id { get; init; }

    /// <summary>
    ///     The fee charged for trades in this pool, expressed in basis points (1/100 of a percent).
    /// </summary>
    [JsonPropertyName("fee_bp")]
    public required int FeeBp { get; init; }

    /// <summary>
    ///     The type of liquidity pool. Currently only constant product pools are supported.
    /// </summary>
    [JsonConverter(typeof(LiquidityPoolTypeEnumJsonConverter))]
    [JsonPropertyName("type")]
    public required LiquidityPoolType.LiquidityPoolTypeEnum Type { get; init; }

    /// <summary>
    ///     The total number of trustlines to this pool's share asset.
    /// </summary>
    [JsonPropertyName("total_trustlines")]
    public required string TotalTrustlines { get; init; }

    /// <summary>
    ///     The total number of pool shares outstanding.
    ///     Represented as a string to preserve precision.
    /// </summary>
    [JsonPropertyName("total_shares")]
    public required string TotalShares { get; init; }

    /// <summary>
    ///     The reserves held in this pool, one for each asset in the trading pair.
    /// </summary>
    [JsonPropertyName("reserves")]
    public required Reserve[] Reserves { get; init; }

    /// <summary>
    ///     Links to related resources for this liquidity pool.
    /// </summary>
    [JsonPropertyName("_links")]
    public required LiquidityPoolResponseLinks Links { get; init; }

    /// <summary>
    ///     The sequence number of the last ledger in which this liquidity pool was modified.
    /// </summary>
    [JsonPropertyName("last_modified_ledger")]
    public required long LastModifiedLedger { get; init; }

    /// <summary>
    ///     The time this liquidity pool was last modified.
    /// </summary>
    [JsonPropertyName("last_modified_time")]
    public required DateTimeOffset LastModifiedTime { get; init; }

    /// <inheritdoc />
    [JsonPropertyName("paging_token")]
    public required string PagingToken { get; init; }

    /// <summary>
    ///     Links to related resources for a liquidity pool.
    /// </summary>
    public sealed class LiquidityPoolResponseLinks
    {
        /// <summary>
        ///     Link to the operations related to this pool.
        /// </summary>
        [JsonPropertyName("operations")]
        public required Link<Page<OperationResponse>> Operations { get; init; }

        /// <summary>
        ///     Link to this liquidity pool resource.
        /// </summary>
        [JsonPropertyName("self")]
        public required Link<LiquidityPoolResponse> Self { get; init; }

        /// <summary>
        ///     Link to the transactions related to this pool.
        /// </summary>
        [JsonPropertyName("transactions")]
        public required Link<Page<TransactionResponse>> Transactions { get; init; }
    }
}