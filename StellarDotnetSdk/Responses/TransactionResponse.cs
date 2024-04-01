using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Responses.Effects;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses;

public class TransactionResponse : Response, IPagingToken
{
    public TransactionResponse()
    {
        // Used by deserializer
    }

    public TransactionResponse(string hash, uint ledger, string createdAt, string sourceAccount, bool successful,
        string pagingToken, long sourceAccountSequence, long feeCharged, int operationCount, string envelopeXdr,
        string resultXdr, string resultMetaXdr, Memo memo, TransactionResponseLinks links)
    {
        Hash = hash;
        Ledger = ledger;
        CreatedAt = createdAt;
        SourceAccount = sourceAccount;
        Successful = successful;
        FeeCharged = feeCharged;
        PagingToken = pagingToken;
        SourceAccountSequence = sourceAccountSequence;
        OperationCount = operationCount;
        EnvelopeXdr = envelopeXdr;
        ResultXdr = resultXdr;
        ResultMetaXdr = resultMetaXdr;
        Memo = memo;
        Links = links;
    }

    public TransactionResponse(string hash, uint ledger, string createdAt, string sourceAccount, string feeAccount,
        bool successful,
        string pagingToken, long sourceAccountSequence, long maxFee, long feeCharged, int operationCount,
        string envelopeXdr,
        string resultXdr, string resultMetaXdr, Memo memo, List<string> signatures,
        FeeBumpTransaction feeBumpTransaction, InnerTransaction innerTransaction, TransactionResponseLinks links)
    {
        Hash = hash;
        Ledger = ledger;
        CreatedAt = createdAt;
        SourceAccount = sourceAccount;
        FeeAccount = feeAccount;
        Successful = successful;
        PagingToken = pagingToken;
        SourceAccountSequence = sourceAccountSequence;
        MaxFee = maxFee;
        FeeCharged = feeCharged;
        OperationCount = operationCount;
        EnvelopeXdr = envelopeXdr;
        ResultXdr = resultXdr;
        ResultMetaXdr = resultMetaXdr;
        Memo = memo;
        Signatures = signatures;
        FeeBumpTx = feeBumpTransaction;
        InnerTx = innerTransaction;
        Links = links;
    }

    [JsonProperty(PropertyName = "hash")] public string Hash { get; private set; }

    [JsonProperty(PropertyName = "ledger")]
    public uint Ledger { get; private set; }

    [JsonProperty(PropertyName = "created_at")]
    public string CreatedAt { get; private set; }

    [JsonProperty(PropertyName = "source_account")]
    public string SourceAccount { get; private set; }

    [JsonProperty(PropertyName = "account_muxed")]
    public string? AccountMuxed { get; private set; }

    [JsonProperty(PropertyName = "account_muxed_id")]
    public ulong? AccountMuxedID { get; private set; }

    [JsonProperty(PropertyName = "fee_account")]
    public string FeeAccount { get; set; }

    [JsonProperty(PropertyName = "fee_account_muxed")]
    public string FeeAccountMuxed { get; set; }

    [JsonProperty(PropertyName = "fee_account_muxed_id")]
    public ulong? FeeAccountMuxedID { get; set; }

    [DefaultValue(true)]
    [JsonProperty(PropertyName = "successful", DefaultValueHandling = DefaultValueHandling.Populate)]
    public bool Successful { get; private set; }

    [JsonProperty(PropertyName = "source_account_sequence")]
    public long SourceAccountSequence { get; private set; }

    [JsonProperty(PropertyName = "fee_charged")]
    public long FeeCharged { get; set; }

    [JsonProperty(PropertyName = "max_fee")]
    public long MaxFee { get; private set; }

    [JsonProperty(PropertyName = "operation_count")]
    public int OperationCount { get; private set; }

    [JsonProperty(PropertyName = "envelope_xdr")]
    public string EnvelopeXdr { get; private set; }

    [JsonProperty(PropertyName = "result_xdr")]
    public string ResultXdr { get; private set; }

    [JsonProperty(PropertyName = "result_meta_xdr")]
    public string ResultMetaXdr { get; private set; }

    [JsonProperty(PropertyName = "signatures")]
    public List<string> Signatures { get; private set; }

    [JsonProperty(PropertyName = "fee_bump_transaction")]
    public FeeBumpTransaction FeeBumpTx { get; set; }

    [JsonProperty(PropertyName = "inner_transaction")]
    public InnerTransaction InnerTx { get; private set; }

    [JsonProperty(PropertyName = "_links")]
    public TransactionResponseLinks Links { get; private set; }

    [JsonProperty(PropertyName = "memo_type")]
    public string MemoType { get; private set; }

    [JsonProperty(PropertyName = "memo")] public string? MemoValue { get; private set; }

    [JsonProperty(PropertyName = "memo_bytes")]
    public string? MemoBytes { get; private set; }

    public Memo Memo
    {
        get
        {
            return MemoType switch
            {
                "none" => Memo.None(),
                "text" => MemoBytes != null ? Memo.Text(Convert.FromBase64String(MemoBytes)) : Memo.Text(MemoValue),
                "id" => Memo.Id(ulong.Parse(MemoValue)),
                "hash" => Memo.Hash(Convert.FromBase64String(MemoValue)),
                "return" => Memo.ReturnHash(Convert.FromBase64String(MemoValue)),
                _ => throw new ArgumentException(nameof(MemoType))
            };
        }
        private set
        {
            switch (value)
            {
                case MemoNone _:
                    MemoType = "none";
                    MemoValue = null;
                    return;
                case MemoText text:
                    MemoType = "text";
                    MemoValue = text.MemoTextValue;
                    return;
                case MemoId id:
                    MemoType = "id";
                    MemoValue = id.IdValue.ToString();
                    return;
                case MemoHash h:
                    MemoType = "hash";
                    MemoValue = Convert.ToBase64String(h.MemoBytes);
                    return;
                case MemoReturnHash r:
                    MemoType = "return";
                    MemoValue = Convert.ToBase64String(r.MemoBytes);
                    return;
                default:
                    throw new ArgumentException(null, nameof(value));
            }
        }
    }

    public TransactionResult Result => TransactionResult.FromXdrBase64(ResultXdr);

    [JsonProperty(PropertyName = "paging_token")]
    public string PagingToken { get; private set; }

    public class FeeBumpTransaction
    {
        public FeeBumpTransaction(string hash, List<string> signatures)
        {
            Hash = hash;
            Signatures = signatures;
        }

        [JsonProperty(PropertyName = "hash")] public string Hash { get; private set; }

        [JsonProperty(PropertyName = "signatures")]
        public List<string> Signatures { get; private set; }
    }

    public class InnerTransaction
    {
        public InnerTransaction(string hash, List<string> signatures, long maxFee)
        {
            Hash = hash;
            Signatures = signatures;
            MaxFee = maxFee;
        }

        [JsonProperty(PropertyName = "hash")] public string Hash { get; private set; }

        [JsonProperty(PropertyName = "signatures")]
        public List<string> Signatures { get; private set; }

        [JsonProperty(PropertyName = "max_fee")]
        public long MaxFee { get; private set; }
    }
}

/// Links connected to transaction.
public class TransactionResponseLinks
{
    public TransactionResponseLinks(Link<AccountResponse> account, Link<Page<EffectResponse>> effects,
        Link<LedgerResponse> ledger, Link<Page<OperationResponse>> operations, Link<TransactionResponse> self,
        Link<TransactionResponse> precedes, Link<TransactionResponse> succeeds)
    {
        Account = account;
        Effects = effects;
        Ledger = ledger;
        Operations = operations;
        Self = self;
        Precedes = precedes;
        Succeeds = succeeds;
    }

    [JsonProperty(PropertyName = "account")]
    public Link<AccountResponse> Account { get; private set; }

    [JsonProperty(PropertyName = "effects")]
    public Link<Page<EffectResponse>> Effects { get; private set; }

    [JsonProperty(PropertyName = "ledger")]
    public Link<LedgerResponse> Ledger { get; private set; }

    [JsonProperty(PropertyName = "operations")]
    public Link<Page<OperationResponse>> Operations { get; private set; }

    [JsonProperty(PropertyName = "precedes")]
    public Link<TransactionResponse> Precedes { get; private set; }

    [JsonProperty(PropertyName = "self")] public Link<TransactionResponse> Self { get; private set; }

    [JsonProperty(PropertyName = "succeeds")]
    public Link<TransactionResponse> Succeeds { get; private set; }
}