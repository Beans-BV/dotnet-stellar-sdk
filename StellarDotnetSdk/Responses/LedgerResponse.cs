using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.Effects;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class LedgerResponse : Response, IPagingToken
{
    [JsonPropertyName("sequence")] public long Sequence { get; init; }

    [JsonPropertyName("hash")] public string Hash { get; init; }

    [JsonPropertyName("prev_hash")] public string PrevHash { get; init; }

    [JsonPropertyName("successful_transaction_count")]
    public int SuccessfulTransactionCount { get; init; }

    [JsonPropertyName("failed_transaction_count")]
    public int? FailedTransactionCount { get; init; }

    [JsonPropertyName("operation_count")] public int OperationCount { get; init; }

    [JsonPropertyName("closed_at")] public string ClosedAt { get; init; }

    [JsonPropertyName("total_coins")] public string TotalCoins { get; init; }

    [JsonPropertyName("fee_pool")] public string FeePool { get; init; }

    [JsonPropertyName("base_fee")] public long BaseFee { get; init; }

    [JsonPropertyName("base_reserve")] public string BaseReserve { get; init; }

    [JsonPropertyName("max_tx_set_size")] public int MaxTxSetSize { get; init; }

    [JsonPropertyName("base_fee_in_stroops")]
    public long BaseFeeInStroops { get; init; }

    [JsonPropertyName("base_reserve_in_stroops")]
    public long BaseReserveInStroops { get; init; }

    [JsonPropertyName("tx_set_operation_count")]
    public int? TxSetOperationCount { get; init; }

    [JsonPropertyName("_links")] public LedgerResponseLinks Links { get; init; }

    [JsonPropertyName("paging_token")] public string PagingToken { get; init; }

    /// Links connected to ledger.
    public class LedgerResponseLinks
    {
        [JsonPropertyName("effects")] public Link<Page<EffectResponse>> Effects { get; init; }

        [JsonPropertyName("operations")] public Link<Page<OperationResponse>> Operations { get; init; }

        [JsonPropertyName("self")] public Link<LedgerResponse> Self { get; init; }

        [JsonPropertyName("transactions")] public Link<Page<TransactionResponse>> Transactions { get; init; }
    }
}