using System;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using SCBytes = StellarDotnetSdk.Soroban.SCBytes;
using SCVal = StellarDotnetSdk.Soroban.SCVal;
using TransactionMeta = StellarDotnetSdk.Soroban.TransactionMeta;

namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Represents detailed information about a Stellar transaction as returned by the Soroban RPC server,
///     including its status, ledger context, and XDR-encoded envelope, result, and metadata.
/// </summary>
public class TransactionInfo
{
    /// <summary>
    ///     Indicates the current status of a transaction retrieved from the Soroban RPC server.
    /// </summary>
    public enum TransactionStatus
    {
        /// <summary>The transaction was not found in the ledger history.</summary>
        NOT_FOUND,

        /// <summary>The transaction was successfully included in a ledger.</summary>
        SUCCESS,

        /// <summary>The transaction failed during execution.</summary>
        FAILED,
    }

    /// <summary>
    ///     The current status of the transaction by hash
    /// </summary>
    public TransactionStatus Status { get; init; }

    /// <summary>
    ///     (optional) The sequence number of the ledger which included the transaction. This field is only present if status
    ///     is SUCCESS or FAILED.
    /// </summary>
    public long? Ledger { get; init; }

    /// <summary>
    ///     (optional) The unix timestamp of when the transaction was included in the ledger. This field is only present if
    ///     status is SUCCESS or FAILED.
    /// </summary>
    public long? CreatedAt { get; init; }

    /// <summary>
    ///     (optional) The index of the transaction among all transactions included in the ledger. This field is only present
    ///     if status is SUCCESS or FAILED.
    /// </summary>
    public int? ApplicationOrder { get; init; }

    /// <summary>
    ///     (optional) Indicates whether the transaction was fee bumped. This field is only present if status is SUCCESS or
    ///     FAILED.
    /// </summary>
    public bool? FeeBump { get; init; }

    /// <summary>
    ///     (optional) A base64 encoded string of the raw TransactionEnvelope XDR struct for this transaction.
    /// </summary>
    public string? EnvelopeXdr { get; init; }

    /// <summary>
    ///     (optional) A base64 encoded string of the raw TransactionResult XDR struct for this transaction. This field is only
    ///     present if status is SUCCESS or FAILED.
    /// </summary>
    public string? ResultXdr { get; init; }

    /// <summary>
    ///     (optional) A base64 encoded string of the raw TransactionMeta XDR struct for this transaction.
    /// </summary>
    public string? ResultMetaXdr { get; init; }

    /// <summary>
    ///     (optional) A base64 encoded slice of xdr.DiagnosticEvent. This is only present if the
    ///     ENABLE_SOROBAN_DIAGNOSTIC_EVENTS has been enabled in the stellar-core config.
    /// </summary>
    [Obsolete("Deprecated in favor of Events.DiagnosticEventsXdr. Will be removed in the next version.")]
    public string[]? DiagnosticEventsXdr { get; init; }

    /// <summary>
    ///     (optional) The events emitted during the transaction execution, including diagnostic, transaction, and contract events.
    /// </summary>
    public Events? Events { get; init; }

    /// <summary>
    ///     (optional) Hex-encoded transaction hash string.
    /// </summary>
    public string? TxHash { get; init; }

    /// <summary>
    ///     (optional) The return value of the Soroban contract invocation, extracted from the transaction metadata.
    ///     Only present for successful transactions that invoked a contract.
    /// </summary>
    public SCVal? ResultValue
    {
        get
        {
            if (Status != TransactionStatus.SUCCESS || ResultMetaXdr == null)
            {
                return null;
            }

            var bytes = Convert.FromBase64String(ResultMetaXdr);
            var reader = new XdrDataInputStream(bytes);
            var meta = Xdr.TransactionMeta.Decode(reader);
            return meta.V4?.SorobanMeta?.ReturnValue == null ? null : SCVal.FromXdr(meta.V4.SorobanMeta.ReturnValue);
        }
    }

    /// <summary>
    ///     Holds the transaction metadata.
    /// </summary>
    public TransactionMeta? TransactionMeta
    {
        get
        {
            if (ResultMetaXdr == null)
            {
                return null;
            }
            try
            {
                return TransactionMeta.FromXdrBase64(ResultMetaXdr);
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    ///     (optional) The hex-encoded WASM hash returned from a contract deployment transaction.
    ///     Only present when the result value is of type SCBytes.
    /// </summary>
    public string? WasmHash
    {
        get
        {
            if (ResultValue is SCBytes bytes)
            {
                return Convert.ToHexString(bytes.InnerValue);
            }

            return null;
        }
    }

    /// <summary>
    ///     (optional) The StrKey contract ID (C...) of a newly created contract.
    ///     Only present when the result value is of type ScContractId.
    /// </summary>
    public string? CreatedContractId
    {
        get
        {
            if (ResultValue is ScContractId contract)
            {
                return contract.InnerValue;
            }
            return null;
        }
    }
}