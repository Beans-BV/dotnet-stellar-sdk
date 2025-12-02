using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.Effects;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses;

public sealed class RootResponseLinks
{
    /// <summary>
    ///     Link to fetch an account.
    /// </summary>
    [JsonPropertyName("account")]
    public required Link<AccountResponse> Account { get; init; }

    /// <summary>
    ///     Link to fetch accounts.
    /// </summary>
    [JsonPropertyName("accounts")]
    public required Link<Page<AccountResponse>> Accounts { get; init; }

    /// <summary>
    ///     Link to fetch transactions for an account.
    /// </summary>
    [JsonPropertyName("account_transactions")]
    public required Link<Page<TransactionResponse>> AccountTransactions { get; init; }

    /// <summary>
    ///     Link to fetch assets.
    /// </summary>
    [JsonPropertyName("assets")]
    public required Link<Page<AssetResponse>> Assets { get; init; }

    /// <summary>
    ///     Link to fetch claimable balances.
    /// </summary>
    [JsonPropertyName("claimable_balances")]
    public required Link<Page<ClaimableBalanceResponse>> ClaimableBalances { get; init; }

    /// <summary>
    ///     Link to fetch effects.
    /// </summary>
    [JsonPropertyName("effects")]
    public required Link<Page<EffectResponse>> Effects { get; init; }

    /// <summary>
    ///     Link to fetch fee stats.
    /// </summary>
    [JsonPropertyName("fee_stats")]
    public required Link<FeeStatsResponse> FeeStats { get; init; }

    /// <summary>
    ///     Link to the friendbot (testnet only).
    /// </summary>
    [JsonPropertyName("friendbot")]
    public Link? Friendbot { get; init; }

    /// <summary>
    ///     Link to fetch a ledger.
    /// </summary>
    [JsonPropertyName("ledger")]
    public required Link<LedgerResponse> Ledger { get; init; }

    /// <summary>
    ///     Link to fetch ledgers.
    /// </summary>
    [JsonPropertyName("ledgers")]
    public required Link<Page<LedgerResponse>> Ledgers { get; init; }

    /// <summary>
    ///     Link to fetch liquidity pools.
    /// </summary>
    [JsonPropertyName("liquidity_pools")]
    public required Link<Page<LiquidityPoolResponse>> LiquidityPools { get; init; }

    /// <summary>
    ///     Link to fetch an offer.
    /// </summary>
    [JsonPropertyName("offer")]
    public required Link<OfferResponse> Offer { get; init; }

    /// <summary>
    ///     Link to fetch offers.
    /// </summary>
    [JsonPropertyName("offers")]
    public required Link<Page<OfferResponse>> Offers { get; init; }

    /// <summary>
    ///     Link to fetch an operation.
    /// </summary>
    [JsonPropertyName("operation")]
    public required Link<OperationResponse> Operation { get; init; }

    /// <summary>
    ///     Link to fetch operations.
    /// </summary>
    [JsonPropertyName("operations")]
    public required Link<Page<OperationResponse>> Operations { get; init; }

    /// <summary>
    ///     Link to fetch the order book.
    /// </summary>
    [JsonPropertyName("order_book")]
    public required Link OrderBook { get; init; }

    /// <summary>
    ///     Link to fetch payments.
    /// </summary>
    [JsonPropertyName("payments")]
    public required Link<Page<PaymentOperationResponse>> Payments { get; init; }

    /// <summary>
    ///     Link to the root endpoint itself.
    /// </summary>
    [JsonPropertyName("self")]
    public required Link<RootResponse> Self { get; init; }

    /// <summary>
    ///     Link to find strict receive payment paths.
    /// </summary>
    [JsonPropertyName("strict_receive_paths")]
    public required Link<Page<PathResponse>> StrictReceivePaths { get; init; }

    /// <summary>
    ///     Link to find strict send payment paths.
    /// </summary>
    [JsonPropertyName("strict_send_paths")]
    public required Link<Page<PathResponse>> StrictSendPaths { get; init; }

    /// <summary>
    ///     Link to fetch trade aggregations.
    /// </summary>
    [JsonPropertyName("trade_aggregations")]
    public required Link<Page<TradeAggregationResponse>> TradeAggregations { get; init; }

    /// <summary>
    ///     Link to fetch trades.
    /// </summary>
    [JsonPropertyName("trades")]
    public required Link<Page<TradeResponse>> Trades { get; init; }

    /// <summary>
    ///     Link to fetch a transaction.
    /// </summary>
    [JsonPropertyName("transaction")]
    public required Link<TransactionResponse> Transaction { get; init; }

    /// <summary>
    ///     Link to fetch transactions.
    /// </summary>
    [JsonPropertyName("transactions")]
    public required Link<Page<TransactionResponse>> Transactions { get; init; }
}