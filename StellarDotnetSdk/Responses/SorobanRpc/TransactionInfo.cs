using System;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using SCBytes = StellarDotnetSdk.Soroban.SCBytes;
using SCVal = StellarDotnetSdk.Soroban.SCVal;
using Soroban_TransactionMetaV3 = StellarDotnetSdk.Soroban.TransactionMetaV3;

namespace StellarDotnetSdk.Responses.SorobanRpc;

public class TransactionInfo
{
    public enum TransactionStatus
    {
        NOT_FOUND,
        SUCCESS,
        FAILED,
    }

    public TransactionInfo(
        TransactionStatus status,
        long? ledger,
        long? createdAt,
        int? applicationOrder,
        bool? feeBump,
        string? envelopeXdr,
        string? resultXdr,
        string? resultMetaXdr,
        string? txHash,
        Events? events = null,
        string[]? diagnosticEventsXdr = null
    )
    {
        Status = status;
        Ledger = ledger;
        CreatedAt = createdAt;
        ApplicationOrder = applicationOrder;
        FeeBump = feeBump;
        EnvelopeXdr = envelopeXdr;
        ResultXdr = resultXdr;
        ResultMetaXdr = resultMetaXdr;
        TxHash = txHash;
        Events = events;
        DiagnosticEventsXdr = diagnosticEventsXdr;
    }

    /// <summary>
    ///     The current status of the transaction by hash
    /// </summary>
    public TransactionStatus Status { get; }

    /// <summary>
    ///     (optional) The sequence number of the ledger which included the transaction. This field is only present if status
    ///     is SUCCESS or FAILED.
    /// </summary>
    public long? Ledger { get; }

    /// <summary>
    ///     (optional) The unix timestamp of when the transaction was included in the ledger. This field is only present if
    ///     status is SUCCESS or FAILED.
    /// </summary>
    public long? CreatedAt { get; }

    /// <summary>
    ///     (optional) The index of the transaction among all transactions included in the ledger. This field is only present
    ///     if status is SUCCESS or FAILED.
    /// </summary>
    public int? ApplicationOrder { get; }

    /// <summary>
    ///     (optional) Indicates whether the transaction was fee bumped. This field is only present if status is SUCCESS or
    ///     FAILED.
    /// </summary>
    public bool? FeeBump { get; }

    /// <summary>
    ///     (optional) A base64 encoded string of the raw TransactionEnvelope XDR struct for this transaction.
    /// </summary>
    public string? EnvelopeXdr { get; }

    /// <summary>
    ///     (optional) A base64 encoded string of the raw TransactionResult XDR struct for this transaction. This field is only
    ///     present if status is SUCCESS or FAILED.
    /// </summary>
    public string? ResultXdr { get; }

    /// <summary>
    ///     (optional) A base64 encoded string of the raw TransactionMeta XDR struct for this transaction.
    /// </summary>
    public string? ResultMetaXdr { get; }

    /// <summary>
    ///     (optional) A base64 encoded slice of xdr.DiagnosticEvent. This is only present if the
    ///     ENABLE_SOROBAN_DIAGNOSTIC_EVENTS has been enabled in the stellar-core config.
    /// </summary>
    [Obsolete("Deprecated in favor of Events.DiagnosticEventsXdr. Will be removed in the next version.")]
    public string[]? DiagnosticEventsXdr { get; }

    public Events? Events { get; }

    /// <summary>
    ///     (optional) Hex-encoded transaction hash string.
    /// </summary>
    public string? TxHash { get; }

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
            return meta.V3?.SorobanMeta?.ReturnValue == null ? null : SCVal.FromXdr(meta.V3.SorobanMeta.ReturnValue);
        }
    }

    /// <summary>
    ///     Holds the diagnostic information, useful for debugging purpose when the transaction fails.
    /// </summary>
    public Soroban_TransactionMetaV3? TransactionMeta
    {
        get
        {
            if (ResultMetaXdr == null)
            {
                return null;
            }
            try
            {
                return Soroban_TransactionMetaV3.FromXdrBase64(ResultMetaXdr);
            }
            catch
            {
                return null;
            }
        }
    }

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