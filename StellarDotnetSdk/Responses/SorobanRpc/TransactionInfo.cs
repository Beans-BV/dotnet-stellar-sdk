using System;
using System.IO;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Soroban;

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
    /// <remarks>
    ///     Stellar RPC tags this field <c>json:"status"</c> with no <c>omitempty</c>, so it is always present;
    ///     <see cref="JsonRequiredAttribute" /> makes that enforceable. An enum is a value type, so without it a
    ///     response carrying no <c>status</c> silently yielded the zero member,
    ///     <see cref="TransactionStatus.NOT_FOUND" />.
    /// </remarks>
    // The property-level [JsonConverter] pins the wire format whichever JsonSerializerOptions the caller
    // supplies: System.Text.Json resolves a property attribute ahead of the options' Converters collection —
    // same pattern as GetEventsRequest.EventFilter.Type. Without it, a consumer deserializing this type with
    // their own options fell back to JsonStringEnumConverter, which maps bare integers by ordinal
    // ("status": 1 read as SUCCESS) and matches case-insensitively.
    [JsonConverter(typeof(TransactionStatusJsonConverter))]
    [JsonRequired]
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
    ///     Both <c>TransactionMetaV3</c> (Protocol 20-22) and <c>TransactionMetaV4</c> (Protocol 23+) are supported.
    ///     Only present for successful transactions that carry Soroban metadata. Returns <c>null</c> — rather than
    ///     throwing — when <see cref="ResultMetaXdr" /> is missing, is not valid base-64, is malformed or truncated,
    ///     carries an unknown union discriminant, is a metadata version that predates Soroban, holds no return
    ///     value, or holds a return value this SDK rejects as unrepresentable. A failure that instead signals a gap
    ///     in this SDK — an <see cref="SCVal" /> type the mapping does not know — still throws, because that is a
    ///     defect here rather than bad input.
    ///     Unlike <see cref="TransactionMeta" />, this property reads the return value straight off the decoded XDR,
    ///     so it still reports the value when some unrelated part of the metadata cannot be mapped to an SDK type.
    /// </summary>
    public SCVal? ResultValue
    {
        get
        {
            if (Status != TransactionStatus.SUCCESS || ResultMetaXdr == null)
            {
                return null;
            }

            Xdr.SCVal? returnValue;
            try
            {
                var reader = new Xdr.XdrDataInputStream(Convert.FromBase64String(ResultMetaXdr));
                var meta = Xdr.TransactionMeta.Decode(reader);

                // Only v3 and v4 carry Soroban metadata; discriminants 0-2 predate Soroban and have no
                // return value to read. A discriminant added by a future protocol would also land on the
                // default arm, so TransactionInfoTest pins the set of discriminants the XDR layer knows:
                // regenerating StellarDotnetSdk.Xdr with a new arm fails that test and forces this switch
                // to be revisited, rather than silently reporting "no return value" (see issue #224).
                returnValue = meta.Discriminant switch
                {
                    3 => meta.V3?.SorobanMeta?.ReturnValue,
                    4 => meta.V4?.SorobanMeta?.ReturnValue,
                    _ => null,
                };

                return returnValue == null ? null : SCVal.FromXdr(returnValue);
            }
            // Every way the payload itself can be bad: invalid base-64, malformed/truncated/over-deep XDR
            // (EndOfStreamException derives from IOException, as does the stream's non-zero-padding check),
            // hostile length prefixes that overflow a signed index (ArgumentOutOfRangeException, an
            // ArgumentException), and a decoded value the SDK rejects as unrepresentable — an SCV_VEC or
            // SCV_MAP whose optional body is absent, or an unmappable address — which SCVal.FromXdr reports
            // as ArgumentException. A failure that instead signals a gap in this SDK — InvalidOperationException
            // for an SCVal type the mapping does not know, or a NullReferenceException from a defect — is
            // deliberately left to propagate rather than being reported to callers as "no return value".
            catch (Exception exception) when (
                exception is FormatException or InvalidDataException or IOException or ArgumentException)
            {
                return null;
            }
        }
    }

    /// <summary>
    ///     Holds the transaction metadata. Returns <c>null</c> when <see cref="ResultMetaXdr" /> is missing, cannot
    ///     be decoded, is a metadata version this SDK does not model (only v3 and v4 are), or contains any structure
    ///     that cannot be mapped to an SDK type. This is all-or-nothing: use <see cref="ResultValue" /> when you only
    ///     need the Soroban return value, as it does not depend on the rest of the metadata converting successfully.
    ///     A failure that says nothing about the payload — an <see cref="OutOfMemoryException" />, or a defect in this
    ///     SDK surfacing as a <see cref="NullReferenceException" /> — propagates rather than being reported here as
    ///     absent metadata.
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
            // Every failure this property documents as null: bad base-64, malformed/truncated/over-deep XDR
            // (EndOfStreamException derives from IOException, as does the stream's non-zero-padding check), a
            // metadata version this SDK does not model plus hostile length prefixes
            // (ArgumentOutOfRangeException, an ArgumentException), and a decoded structure that cannot be
            // mapped onto an SDK type (ArgumentException / InvalidOperationException). A bare catch also
            // swallowed failures that say nothing about the payload — an OutOfMemoryException on a large
            // metadata graph, or a NullReferenceException from a genuine defect in this SDK — and reported
            // them to callers as "no metadata". Those now propagate, matching ResultValue's stance that an
            // SDK bug is not bad input.
            catch (Exception exception) when (
                exception is FormatException or InvalidDataException or IOException
                    or ArgumentException or InvalidOperationException)
            {
                return null;
            }
        }
    }

    /// <summary>
    ///     (optional) The hex-encoded WASM hash returned from a contract deployment transaction.
    ///     Only present when <see cref="ResultValue" /> is of type <see cref="SCBytes" />.
    /// </summary>
    public string? WasmHash
    {
        get
        {
            if (ResultValue is SCBytes bytes)
            {
                return Util.BytesToHex(bytes.InnerValue);
            }

            return null;
        }
    }

    /// <summary>
    ///     (optional) The StrKey contract ID (C...) of a newly created contract.
    ///     Only present when <see cref="ResultValue" /> is of type <see cref="ScContractId" />.
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