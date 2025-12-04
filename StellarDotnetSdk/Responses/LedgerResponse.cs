using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.Effects;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents a ledger from the Stellar network.
///     A ledger is equivalent to a block in other blockchain systems and contains all transactions
///     that were applied during that consensus round.
/// </summary>
public sealed class LedgerResponse : Response, IPagingToken
{
    /// <summary>
    ///     A unique identifier for this ledger.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    ///     The sequence number of this ledger.
    /// </summary>
    [JsonPropertyName("sequence")]
    public required long Sequence { get; init; }

    /// <summary>
    ///     A hex-encoded SHA-256 hash of the ledger's XDR-encoded form.
    /// </summary>
    [JsonPropertyName("hash")]
    public required string Hash { get; init; }

    /// <summary>
    ///     The hash of the previous ledger in the chain.
    /// </summary>
    [JsonPropertyName("prev_hash")]
    public required string PrevHash { get; init; }

    /// <summary>
    ///     The number of successful transactions in this ledger.
    /// </summary>
    [JsonPropertyName("successful_transaction_count")]
    public required int SuccessfulTransactionCount { get; init; }

    /// <summary>
    ///     The number of failed transactions in this ledger.
    /// </summary>
    [JsonPropertyName("failed_transaction_count")]
    public required int FailedTransactionCount { get; init; }

    /// <summary>
    ///     The total number of operations in all successful transactions in this ledger.
    /// </summary>
    [JsonPropertyName("operation_count")]
    public required int OperationCount { get; init; }

    /// <summary>
    ///     The time when this ledger was closed.
    /// </summary>
    [JsonPropertyName("closed_at")]
    public required DateTimeOffset ClosedAt { get; init; }

    /// <summary>
    ///     The total number of lumens in circulation.
    ///     Represented as a string to preserve precision.
    /// </summary>
    [JsonPropertyName("total_coins")]
    public required string TotalCoins { get; init; }

    /// <summary>
    ///     The sum of all transaction fees (in lumens) since the last inflation operation.
    ///     Represented as a string to preserve precision.
    /// </summary>
    [JsonPropertyName("fee_pool")]
    public required string FeePool { get; init; }

    /// <summary>
    ///     The maximum number of transactions allowed in this ledger's transaction set.
    /// </summary>
    [JsonPropertyName("max_tx_set_size")]
    public required int MaxTxSetSize { get; init; }

    /// <summary>
    ///     The base fee for operations in this ledger (in stroops).
    /// </summary>
    [JsonPropertyName("base_fee_in_stroops")]
    public required long BaseFeeInStroops { get; init; }

    /// <summary>
    ///     The minimum account balance required to exist on the network (in stroops).
    /// </summary>
    [JsonPropertyName("base_reserve_in_stroops")]
    public required long BaseReserveInStroops { get; init; }

    /// <summary>
    ///     The number of operations in the transaction set (may differ from operation_count
    ///     if there are failed transactions).
    /// </summary>
    [JsonPropertyName("tx_set_operation_count")]
    public required int TxSetOperationCount { get; init; }

    /// <summary>
    ///     The protocol version that the Stellar network was running when this ledger was committed.
    /// </summary>
    [JsonPropertyName("protocol_version")]
    public required int ProtocolVersion { get; init; }

    /// <summary>
    ///     A base64 encoded string of the raw LedgerHeader xdr struct for this ledger.
    /// </summary>
    [JsonPropertyName("header_xdr")]
    public required string HeaderXdr { get; init; }

    /// <summary>
    ///     Links to related resources for this ledger.
    /// </summary>
    [JsonPropertyName("_links")]
    public required LedgerResponseLinks Links { get; init; }

    /// <inheritdoc />
    [JsonPropertyName("paging_token")]
    public required string PagingToken { get; init; }

    /// <summary>
    ///     Links to related resources for a ledger.
    /// </summary>
    public sealed class LedgerResponseLinks
    {
        /// <summary>
        ///     Link to the effects in this ledger.
        /// </summary>
        [JsonPropertyName("effects")]
        public required Link<Page<EffectResponse>> Effects { get; init; }

        /// <summary>
        ///     Link to the operations in this ledger.
        /// </summary>
        [JsonPropertyName("operations")]
        public required Link<Page<OperationResponse>> Operations { get; init; }

        /// <summary>
        ///     Link to the payments in this ledger.
        /// </summary>
        [JsonPropertyName("payments")]
        public required Link<Page<OperationResponse>> Payments { get; init; }

        /// <summary>
        ///     Link to this ledger resource.
        /// </summary>
        [JsonPropertyName("self")]
        public required Link<LedgerResponse> Self { get; init; }

        /// <summary>
        ///     Link to the transactions in this ledger.
        /// </summary>
        [JsonPropertyName("transactions")]
        public required Link<Page<TransactionResponse>> Transactions { get; init; }
    }
}