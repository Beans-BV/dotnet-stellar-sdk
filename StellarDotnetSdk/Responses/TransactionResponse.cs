using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents a transaction on the Stellar network.
///     Contains transaction metadata, results, and links to related resources.
/// </summary>
public sealed class TransactionResponse : Response, IPagingToken
{
    /// <summary>
    ///     A unique identifier for this transaction.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    ///     The hash of the transaction.
    /// </summary>
    [JsonPropertyName("hash")]
    public required string Hash { get; init; }

    /// <summary>
    ///     The ledger sequence number in which this transaction was included.
    /// </summary>
    [JsonPropertyName("ledger")]
    public required long Ledger { get; init; }

    /// <summary>
    /// An ISO 8601 formatted string of when the transaction was created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    ///     The account that originated this transaction.
    /// </summary>
    [JsonPropertyName("source_account")]
    public required string SourceAccount { get; init; }

    /// <summary>
    ///     The account that paid the transaction fee.
    ///     For fee bump transactions, this differs from the source account.
    /// </summary>
    [JsonPropertyName("fee_account")]
    public required string FeeAccount { get; init; }

    /// <summary>
    ///     The muxed account representation of the fee account, if applicable.
    /// </summary>
    [JsonPropertyName("fee_account_muxed")]
    public string? FeeAccountMuxed { get; init; }

    /// <summary>
    ///     Whether the transaction was successful.
    /// </summary>
    [JsonPropertyName("successful")]
    public required bool Successful { get; init; }

    /// <summary>
    ///     The sequence number of the source account when the transaction was submitted.
    /// </summary>
    [JsonPropertyName("source_account_sequence")]
    public required long SourceAccountSequence { get; init; }

    /// <summary>
    ///     The actual fee paid for the transaction in stroops.
    /// </summary>
    [JsonPropertyName("fee_charged")]
    public required long FeeCharged { get; init; }

    /// <summary>
    ///     The maximum fee the source account was willing to pay in stroops.
    /// </summary>
    [JsonPropertyName("max_fee")]
    public required long MaxFee { get; init; }

    /// <summary>
    ///     The number of operations in this transaction.
    /// </summary>
    [JsonPropertyName("operation_count")]
    public required int OperationCount { get; init; }

    /// <summary>
    ///     The XDR-encoded transaction envelope.
    /// </summary>
    [JsonPropertyName("envelope_xdr")]
    public required string EnvelopeXdr { get; init; }

    /// <summary>
    ///     The XDR-encoded transaction result.
    /// </summary>
    [JsonPropertyName("result_xdr")]
    public required string ResultXdr { get; init; }

    /// <summary>
    ///     A base64 encoded string of the raw LedgerEntryChanges XDR struct
    /// produced by taking fees for this transaction.
    /// </summary>
    [JsonPropertyName("fee_meta_xdr")]
    public required string FeeMetaXdr { get; init; }

    /// <summary>
    ///     The base64-encoded signatures applied to this transaction.
    /// </summary>
    [JsonPropertyName("signatures")]
    public required List<string> Signatures { get; init; }

    /// <summary>
    ///     A set of transaction preconditions affecting the validity of this transaction.
    /// </summary>
    [JsonPropertyName("preconditions")]
    public TransactionResponsePreconditions? Preconditions { get; init; }

    /// <summary>
    ///     The fee bump transaction details, if this transaction was fee bumped.
    /// </summary>
    [JsonPropertyName("fee_bump_transaction")]
    public FeeBumpTransaction? FeeBumpTx { get; init; }

    /// <summary>
    ///     The inner transaction details, if this is a fee bump transaction.
    /// </summary>
    [JsonPropertyName("inner_transaction")]
    public InnerTransaction? InnerTx { get; init; }

    /// <summary>
    ///     Links to related resources for this transaction.
    /// </summary>
    [JsonPropertyName("_links")]
    public required TransactionResponseLinks Links { get; init; }

    /// <summary>
    ///     The type of memo attached to this transaction: "none", "text", "id", "hash", or "return".
    /// </summary>
    [JsonPropertyName("memo_type")]
    public required string MemoType { get; init; }

    /// <summary>
    ///     The muxed account ID of the source account, if applicable.
    /// </summary>
    [JsonPropertyName("account_muxed_id")]
    public ulong? AccountMuxedId { get; init; }

    /// <summary>
    ///     The muxed account representation of the source account, if applicable.
    /// </summary>
    [JsonPropertyName("account_muxed")]
    public string? AccountMuxed { get; init; }

    /// <summary>
    ///     The muxed account ID of the fee account, if applicable.
    /// </summary>
    [JsonPropertyName("fee_account_muxed_id")]
    public ulong? FeeAccountMuxedId { get; init; }

    /// <summary>
    ///     The memo value attached to this transaction.
    ///     For text memos, this is the text content.
    ///     For id memos, this is the numeric ID as a string.
    ///     For hash/return memos, this is the base64-encoded hash.
    /// </summary>
    [JsonPropertyName("memo")]
    public string? MemoValue { get; init; }

    /// <summary>
    ///     The raw bytes of the memo, base64-encoded.
    ///     Present for text memos to preserve exact byte content.
    /// </summary>
    [JsonPropertyName("memo_bytes")]
    public string? MemoBytes { get; init; }

    /// <summary>
    ///     The parsed memo object for this transaction.
    /// </summary>
    [JsonIgnore]
    public Memo Memo
    {
        get
        {
            return MemoType switch
            {
                "none" => Memo.None(),
                "text" => MemoBytes != null ? Memo.Text(Convert.FromBase64String(MemoBytes)) :
                    MemoValue != null ? Memo.Text(MemoValue) :
                    throw new InvalidOperationException("MemoValue is required for text memo"),
                "id" => MemoValue != null
                    ? Memo.Id(ulong.Parse(MemoValue))
                    : throw new InvalidOperationException("MemoValue is required for id memo"),
                "hash" => MemoValue != null
                    ? Memo.Hash(Convert.FromBase64String(MemoValue))
                    : throw new InvalidOperationException("MemoValue is required for hash memo"),
                "return" => MemoValue != null
                    ? Memo.ReturnHash(Convert.FromBase64String(MemoValue))
                    : throw new InvalidOperationException("MemoValue is required for return memo"),
                _ => throw new InvalidOperationException($"Unknown memo type: {MemoType}"),
            };
        }
        init
        {
            switch (value)
            {
                case MemoNone:
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
                    throw new ArgumentException("Unknown memo type", nameof(value));
            }
        }
    }

    /// <summary>
    ///     The parsed transaction result.
    /// </summary>
    public TransactionResult Result => TransactionResult.FromXdrBase64(ResultXdr);

    /// <summary>
    ///     A cursor value for use in pagination.
    /// </summary>
    [JsonPropertyName("paging_token")]
    public required string PagingToken { get; init; }

    /// <summary>
    ///     Represents the fee bump transaction wrapper for a fee-bumped transaction.
    /// </summary>
    public sealed class FeeBumpTransaction
    {
        /// <summary>
        ///     The hash of the fee bump transaction.
        /// </summary>
        [JsonPropertyName("hash")]
        public required string Hash { get; init; }

        /// <summary>
        ///     The base64-encoded signatures on the fee bump transaction.
        /// </summary>
        [JsonPropertyName("signatures")]
        public required List<string> Signatures { get; init; }
    }

    /// <summary>
    ///     Represents the inner transaction within a fee bump transaction.
    /// </summary>
    public sealed class InnerTransaction
    {
        /// <summary>
        ///     The hash of the inner transaction.
        /// </summary>
        [JsonPropertyName("hash")]
        public required string Hash { get; init; }

        /// <summary>
        ///     The base64-encoded signatures on the inner transaction.
        /// </summary>
        [JsonPropertyName("signatures")]
        public required List<string> Signatures { get; init; }

        /// <summary>
        ///     The maximum fee of the inner transaction in stroops.
        /// </summary>
        [JsonPropertyName("max_fee")]
        public required long MaxFee { get; init; }
    }
}