using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Responses.Effects;
using StellarDotnetSdk.Responses.Operations;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class TransactionResponse : Response, IPagingToken
{
    [JsonProperty(PropertyName = "hash")] public string Hash { get; init; }

    [JsonProperty(PropertyName = "ledger")]
    public uint Ledger { get; init; }

    [JsonProperty(PropertyName = "created_at")]
    public DateTime CreatedAt { get; init; }

    [JsonProperty(PropertyName = "source_account")]
    public string SourceAccount { get; init; }

    [JsonProperty(PropertyName = "fee_account")]
    public string FeeAccount { get; set; }

    [JsonProperty(PropertyName = "fee_account_muxed")]
    public string FeeAccountMuxed { get; set; }

    [DefaultValue(true)]
    [JsonProperty(PropertyName = "successful", DefaultValueHandling = DefaultValueHandling.Populate)]
    public bool Successful { get; init; }

    [JsonProperty(PropertyName = "source_account_sequence")]
    public long SourceAccountSequence { get; init; }

    [JsonProperty(PropertyName = "fee_charged")]
    public long FeeCharged { get; set; }

    [JsonProperty(PropertyName = "max_fee")]
    public long MaxFee { get; init; }

    [JsonProperty(PropertyName = "operation_count")]
    public int OperationCount { get; init; }

    [JsonProperty(PropertyName = "envelope_xdr")]
    public string EnvelopeXdr { get; init; }

    [JsonProperty(PropertyName = "result_xdr")]
    public string ResultXdr { get; init; }

    [JsonProperty(PropertyName = "result_meta_xdr")]
    public string ResultMetaXdr { get; init; }

    [JsonProperty(PropertyName = "signatures")]
    public List<string> Signatures { get; init; }

    [JsonProperty(PropertyName = "fee_bump_transaction")]
    public FeeBumpTransaction FeeBumpTx { get; set; }

    [JsonProperty(PropertyName = "inner_transaction")]
    public InnerTransaction InnerTx { get; init; }

    [JsonProperty(PropertyName = "_links")]
    public TransactionResponseLinks Links { get; init; }

    [JsonProperty(PropertyName = "memo_type")]
    public string MemoType { get; init; }
#nullable restore

    [JsonProperty(PropertyName = "account_muxed_id")]
    public ulong? AccountMuxedID { get; init; }

    [JsonProperty(PropertyName = "account_muxed")]
    public string? AccountMuxed { get; init; }

    [JsonProperty(PropertyName = "fee_account_muxed_id")]
    public ulong? FeeAccountMuxedID { get; set; }

    [JsonProperty(PropertyName = "memo")] public string? MemoValue { get; init; }

    [JsonProperty(PropertyName = "memo_bytes")]
    public string? MemoBytes { get; init; }

    public Memo Memo
    {
        get
        {
            return MemoType switch
            {
                "none" => Memo.None(),
                "text" => MemoBytes != null ? Memo.Text(Convert.FromBase64String(MemoBytes)) :
                    MemoValue != null ? Memo.Text(MemoValue) : throw new ArgumentNullException(nameof(MemoValue)),
                "id" => MemoValue != null
                    ? Memo.Id(ulong.Parse(MemoValue))
                    : throw new ArgumentNullException(nameof(MemoValue)),
                "hash" => MemoValue != null
                    ? Memo.Hash(Convert.FromBase64String(MemoValue))
                    : throw new ArgumentNullException(nameof(MemoValue)),
                "return" => MemoValue != null
                    ? Memo.ReturnHash(Convert.FromBase64String(MemoValue))
                    : throw new ArgumentNullException(nameof(MemoValue)),
                _ => throw new ArgumentException(nameof(MemoType)),
            };
        }
        init
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
    public string PagingToken { get; init; }

    public class FeeBumpTransaction
    {
        public FeeBumpTransaction(string hash, List<string> signatures)
        {
            Hash = hash;
            Signatures = signatures;
        }

        [JsonProperty(PropertyName = "hash")] public string Hash { get; init; }

        [JsonProperty(PropertyName = "signatures")]
        public List<string> Signatures { get; init; }
    }

    public class InnerTransaction
    {
        public InnerTransaction(string hash, List<string> signatures, long maxFee)
        {
            Hash = hash;
            Signatures = signatures;
            MaxFee = maxFee;
        }

        [JsonProperty(PropertyName = "hash")] public string Hash { get; init; }

        [JsonProperty(PropertyName = "signatures")]
        public List<string> Signatures { get; init; }

        [JsonProperty(PropertyName = "max_fee")]
        public long MaxFee { get; init; }
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
    public Link<AccountResponse> Account { get; init; }

    [JsonProperty(PropertyName = "effects")]
    public Link<Page<EffectResponse>> Effects { get; init; }

    [JsonProperty(PropertyName = "ledger")]
    public Link<LedgerResponse> Ledger { get; init; }

    [JsonProperty(PropertyName = "operations")]
    public Link<Page<OperationResponse>> Operations { get; init; }

    [JsonProperty(PropertyName = "precedes")]
    public Link<TransactionResponse> Precedes { get; init; }

    [JsonProperty(PropertyName = "self")] public Link<TransactionResponse> Self { get; init; }

    [JsonProperty(PropertyName = "succeeds")]
    public Link<TransactionResponse> Succeeds { get; init; }
}