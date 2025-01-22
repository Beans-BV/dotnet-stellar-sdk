using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Responses.Effects;
using StellarDotnetSdk.Responses.Operations;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class TransactionResponse : Response, IPagingToken
{
    [JsonPropertyName("hash")] public string Hash { get; init; }

    [JsonPropertyName("ledger")]
    public long Ledger { get; init; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    [JsonPropertyName("source_account")]
    public string SourceAccount { get; init; }


    [JsonPropertyName("fee_account")]
    public string FeeAccount { get; set; }

    [JsonPropertyName("fee_account_muxed")]
    public string FeeAccountMuxed { get; set; }

    [JsonPropertyName("successful")]
    public bool Successful { get; init; } = true;

    [JsonPropertyName("source_account_sequence")]
    public long SourceAccountSequence { get; init; }

    [JsonPropertyName("fee_charged")]
    public long FeeCharged { get; set; }

    [JsonPropertyName("max_fee")]
    public long MaxFee { get; init; }

    [JsonPropertyName("operation_count")]
    public int OperationCount { get; init; }

    [JsonPropertyName("envelope_xdr")]
    public string EnvelopeXdr { get; init; }

    [JsonPropertyName("result_xdr")]
    public string ResultXdr { get; init; }

    [JsonPropertyName("result_meta_xdr")]
    public string ResultMetaXdr { get; init; }

    [JsonPropertyName("signatures")]
    public List<string> Signatures { get; init; }

    [JsonPropertyName("fee_bump_transaction")]
    public FeeBumpTransaction FeeBumpTx { get; set; }

    [JsonPropertyName("inner_transaction")]
    public InnerTransaction InnerTx { get; init; }

    [JsonPropertyName("_links")]
    public TransactionResponseLinks Links { get; init; }

    [JsonPropertyName("memo_type")]
    public string MemoType { get; init; }
#nullable restore

    [JsonPropertyName("account_muxed_id")]
    public ulong? AccountMuxedId { get; init; }

    [JsonPropertyName("account_muxed")]
    public string? AccountMuxed { get; init; }

    [JsonPropertyName("fee_account_muxed_id")]
    public ulong? FeeAccountMuxedId { get; set; }

    [JsonPropertyName("memo")] public string? MemoValue { get; init; }

    [JsonPropertyName("memo_bytes")]
    public string? MemoBytes { get; init; }

    [JsonIgnore]
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

    [JsonPropertyName("paging_token")]
    public string PagingToken { get; init; }

    public class FeeBumpTransaction
    {
        public FeeBumpTransaction(string hash, List<string> signatures)
        {
            Hash = hash;
            Signatures = signatures;
        }

        [JsonPropertyName("hash")] public string Hash { get; init; }

        [JsonPropertyName("signatures")]
        public List<string> Signatures { get; init; }
    }

    public class InnerTransaction
    {
        [JsonPropertyName("hash")] public string Hash { get; init; }

        [JsonPropertyName("signatures")]
        public List<string> Signatures { get; init; }

        [JsonPropertyName("max_fee")]
        public long MaxFee { get; init; }
    }
}

/// Links connected to transaction.
public class TransactionResponseLinks
{
    [JsonPropertyName("account")]
    public Link<AccountResponse> Account { get; init; }

    [JsonPropertyName("effects")]
    public Link<Page<EffectResponse>> Effects { get; init; }

    [JsonPropertyName("ledger")]
    public Link<LedgerResponse> Ledger { get; init; }

    [JsonPropertyName("operations")]
    public Link<Page<OperationResponse>> Operations { get; init; }

    [JsonPropertyName("precedes")]
    public Link<TransactionResponse> Precedes { get; init; }

    [JsonPropertyName("self")] public Link<TransactionResponse> Self { get; init; }

    [JsonPropertyName("succeeds")]
    public Link<TransactionResponse> Succeeds { get; init; }
}