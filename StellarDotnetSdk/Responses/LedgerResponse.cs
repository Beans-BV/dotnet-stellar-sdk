using Newtonsoft.Json;
using StellarDotnetSdk.Responses.Effects;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class LedgerResponse : Response, IPagingToken
{
    [JsonProperty(PropertyName = "sequence")]
    public uint Sequence { get; init; }

    [JsonProperty(PropertyName = "hash")] public string Hash { get; init; }

    [JsonProperty(PropertyName = "prev_hash")]
    public string PrevHash { get; init; }

    [JsonProperty(PropertyName = "successful_transaction_count")]
    public int SuccessfulTransactionCount { get; init; }

    [JsonProperty(PropertyName = "failed_transaction_count")]
    public int? FailedTransactionCount { get; init; }

    [JsonProperty(PropertyName = "operation_count")]
    public int OperationCount { get; init; }

    [JsonProperty(PropertyName = "closed_at")]
    public string ClosedAt { get; init; }

    [JsonProperty(PropertyName = "total_coins")]
    public string TotalCoins { get; init; }

    [JsonProperty(PropertyName = "fee_pool")]
    public string FeePool { get; init; }

    [JsonProperty(PropertyName = "base_fee")]
    public long BaseFee { get; init; }

    [JsonProperty(PropertyName = "base_reserve")]
    public string BaseReserve { get; init; }

    [JsonProperty(PropertyName = "max_tx_set_size")]
    public int MaxTxSetSize { get; init; }

    [JsonProperty(PropertyName = "base_fee_in_stroops")]
    public string BaseFeeInStroops { get; init; }

    [JsonProperty(PropertyName = "base_reserve_in_stroops")]
    public string BaseReserveInStroops { get; init; }

    [JsonProperty(PropertyName = "tx_set_operation_count")]
    public int? TxSetOperationCount { get; init; }

    [JsonProperty(PropertyName = "_links")]
    public LedgerResponseLinks Links { get; init; }

    [JsonProperty(PropertyName = "paging_token")]
    public string PagingToken { get; init; }

    /// Links connected to ledger.
    public class LedgerResponseLinks
    {
        [JsonProperty(PropertyName = "effects")]
        public Link<Page<EffectResponse>> Effects { get; init; }

        [JsonProperty(PropertyName = "operations")]
        public Link<Page<OperationResponse>> Operations { get; init; }

        [JsonProperty(PropertyName = "self")] public Link<LedgerResponse> Self { get; init; }

        [JsonProperty(PropertyName = "transactions")]
        public Link<Page<TransactionResponse>> Transactions { get; init; }
    }
}