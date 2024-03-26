using Newtonsoft.Json;
using StellarDotnetSdk.Responses.Effects;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses;

public class LedgerResponse : Response, IPagingToken
{
    public LedgerResponse(uint sequence, string hash, string pagingToken, string prevHash,
        int? failedTransactionCount, int successfulTransactionCount, int operationCount, string closedAt,
        string totalCoins, string feePool, long baseFee, string baseReserve, string baseFeeInStroops,
        string baseReserveInStroops, int maxTxSetSize, int? txSetOperationCount, LedgerResponseLinks links)
    {
        Sequence = sequence;
        Hash = hash;
        PagingToken = pagingToken;
        PrevHash = prevHash;
        FailedTransactionCount = failedTransactionCount;
        SuccessfulTransactionCount = successfulTransactionCount;
        OperationCount = operationCount;
        ClosedAt = closedAt;
        TotalCoins = totalCoins;
        FeePool = feePool;
        BaseFee = baseFee;
        BaseFeeInStroops = baseFeeInStroops;
        BaseReserve = baseReserve;
        BaseReserveInStroops = baseReserveInStroops;
        MaxTxSetSize = maxTxSetSize;
        TxSetOperationCount = txSetOperationCount;
        Links = links;
    }

    [JsonProperty(PropertyName = "sequence")]
    public uint Sequence { get; private set; }

    [JsonProperty(PropertyName = "hash")] public string Hash { get; private set; }

    [JsonProperty(PropertyName = "prev_hash")]
    public string PrevHash { get; private set; }

    [JsonProperty(PropertyName = "successful_transaction_count")]
    public int SuccessfulTransactionCount { get; private set; }

    [JsonProperty(PropertyName = "failed_transaction_count")]
    public int? FailedTransactionCount { get; private set; }

    [JsonProperty(PropertyName = "operation_count")]
    public int OperationCount { get; private set; }

    [JsonProperty(PropertyName = "closed_at")]
    public string ClosedAt { get; private set; }

    [JsonProperty(PropertyName = "total_coins")]
    public string TotalCoins { get; private set; }

    [JsonProperty(PropertyName = "fee_pool")]
    public string FeePool { get; private set; }

    [JsonProperty(PropertyName = "base_fee")]
    public long BaseFee { get; private set; }

    [JsonProperty(PropertyName = "base_reserve")]
    public string BaseReserve { get; private set; }

    [JsonProperty(PropertyName = "max_tx_set_size")]
    public int MaxTxSetSize { get; private set; }

    [JsonProperty(PropertyName = "base_fee_in_stroops")]
    public string BaseFeeInStroops { get; private set; }

    [JsonProperty(PropertyName = "base_reserve_in_stroops")]
    public string BaseReserveInStroops { get; private set; }

    [JsonProperty(PropertyName = "tx_set_operation_count")]
    public int? TxSetOperationCount { get; private set; }

    [JsonProperty(PropertyName = "_links")]
    public LedgerResponseLinks Links { get; private set; }

    [JsonProperty(PropertyName = "paging_token")]
    public string PagingToken { get; private set; }

    /// Links connected to ledger.
    public class LedgerResponseLinks
    {
        public LedgerResponseLinks(Link<Page<EffectResponse>> effects, Link<Page<OperationResponse>> operations,
            Link<LedgerResponse> self, Link<Page<TransactionResponse>> transactions)
        {
            Effects = effects;
            Operations = operations;
            Self = self;
            Transactions = transactions;
        }

        [JsonProperty(PropertyName = "effects")]
        public Link<Page<EffectResponse>> Effects { get; private set; }

        [JsonProperty(PropertyName = "operations")]
        public Link<Page<OperationResponse>> Operations { get; private set; }

        [JsonProperty(PropertyName = "self")] public Link<LedgerResponse> Self { get; private set; }

        [JsonProperty(PropertyName = "transactions")]
        public Link<Page<TransactionResponse>> Transactions { get; private set; }
    }
}