using System;
using System.IO;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Soroban;
// Deliberately NOT `using StellarDotnetSdk.Exceptions`: that namespace declares its own FormatException,
// which would silently shadow System.FormatException in IsPayloadFailure below and stop it recognising
// the invalid-base64 failures it exists for. The one SDK exception type needed here is aliased in instead.
using AssetCodeLengthInvalidException = StellarDotnetSdk.Exceptions.AssetCodeLengthInvalidException;

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
    /// <remarks>
    ///     The type-level <see cref="JsonConverterAttribute" /> is the third and weakest of the three places the
    ///     strict converter is attached, and it covers the case the other two miss: the enum travelling on its
    ///     own — a caller's own DTO field, a persisted status column, a queue message — under a
    ///     <see cref="System.Text.Json.JsonSerializerOptions" /> that registers no converter for it. Without it
    ///     the built-in enum handling maps a bare integer by ordinal, and ordinal 1 here is
    ///     <see cref="SUCCESS" /> — so a corrupted stored record read as a confirmed transaction. See
    ///     <see cref="Requests.SorobanRpc.EventFilterType" /> for the full three-tier resolution order.
    /// </remarks>
    [JsonConverter(typeof(TransactionStatusJsonConverter))]
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
    //
    // That covers the converter only. This type carries no [JsonPropertyName], so binding the wire's
    // lowercase "status" to this property still needs PropertyNameCaseInsensitive, which JsonOptions.
    // DefaultOptions sets and a bare JsonSerializerOptions does not. Under genuinely bare options the
    // property never binds and [JsonRequired] then fails the whole response with "missing required
    // properties including: 'Status'" — so a consumer deserializing these types themselves must match
    // property names case-insensitively, not merely leave the converter alone.
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
    ///     defect here rather than bad input; no payload can currently provoke that, since the generated decoder
    ///     rejects an unknown <c>SCValType</c> first, so treat it as a standing guarantee rather than a case to
    ///     handle.
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
                // return value to read, and they are the only values that reach the default arm today —
                // Xdr.TransactionMeta.Decode rejects anything outside 0-4 with InvalidDataException before
                // this switch runs, so an unknown discriminant is answered by the catch clause below, not
                // here. The risk this guards is therefore the *next* protocol: once StellarDotnetSdk.Xdr is
                // regenerated with a new arm, that version starts decoding cleanly and would fall to the
                // default arm as a silent "no return value". TransactionInfoTest pins the discriminant set
                // so that regeneration fails a test and forces this switch to be revisited (see issue #224).
                returnValue = meta.Discriminant switch
                {
                    3 => meta.V3?.SorobanMeta?.ReturnValue,
                    4 => meta.V4?.SorobanMeta?.ReturnValue,
                    _ => null,
                };

                return returnValue == null ? null : SCVal.FromXdr(returnValue);
            }
            // Every way the payload itself can be bad — see IsPayloadFailure — except InvalidOperationException,
            // which this property deliberately lets propagate: it signals a gap in this SDK's own mapping rather
            // than bad input, and reporting an SDK defect to callers as "no return value" would hide it. The
            // exclusion is stated as a deviation from the shared set rather than by re-listing the other types,
            // so the two properties cannot drift apart over what counts as a bad payload.
            //
            // Note that on the route this property takes — SCVal.FromXdr on the decoded return value — that
            // exclusion is currently theoretical: SCVal.FromXdr's "Unknown SCVal type" arm covers all 22
            // SCValTypeEnum members, and the generated SCValType.Decode rejects every other wire value with
            // InvalidDataException first, so no payload reaches it. It is kept as a standing guarantee for
            // whenever the generated XDR layer gains a member ahead of the mapping.
            catch (Exception exception) when (
                IsPayloadFailure(exception) && exception is not InvalidOperationException)
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
            // Every failure this property documents as null — see IsPayloadFailure. Unlike ResultValue this
            // property does report an unmappable structure as absent metadata, InvalidOperationException
            // included: it is an all-or-nothing view, so "some part of this graph has no SDK representation"
            // is the same answer as "no metadata" to every caller. A bare catch also swallowed failures that
            // say nothing about the payload — an OutOfMemoryException on a large metadata graph, or a
            // NullReferenceException from a genuine defect in this SDK — and reported them to callers as "no
            // metadata". Those now propagate.
            catch (Exception exception) when (IsPayloadFailure(exception))
            {
                return null;
            }
        }
    }

    /// <summary>
    ///     Recognizes the exceptions that decoding a server-supplied base64 XDR blob can produce, so that both
    ///     <see cref="ResultValue" /> and <see cref="TransactionMeta" /> answer "bad payload" from one declaration
    ///     of the set rather than two hand-maintained lists that drift apart.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The list is transcribed from <c>SimulateTransactionResponse.IsXdrDecodeFailure</c>, which derived it
    ///         empirically from a fuzz of the same decoders — the generated <c>StellarDotnetSdk.Xdr</c> readers plus
    ///         the SDK's own <c>FromXdr</c> dispatch (<c>SCVal</c>, <c>ScAddress</c>, <c>Asset</c>,
    ///         <c>TrustlineAsset</c>, …). The two copies must stay in step; they are separate only because that
    ///         property normalizes to <see cref="InvalidDataException" /> where these two return <c>null</c>.
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="FormatException" /> — invalid base-64, or a length prefix that runs off the buffer.
    ///         </item>
    ///         <item>
    ///             <see cref="InvalidDataException" /> — an unknown enum or union discriminant, an over-large
    ///             element count, or the maximum decoding depth being reached.
    ///         </item>
    ///         <item>
    ///             <see cref="IOException" /> (and <see cref="EndOfStreamException" />, which derives from it) —
    ///             truncated input, or non-zero opaque padding.
    ///         </item>
    ///         <item>
    ///             <see cref="IndexOutOfRangeException" /> — a read past the end of the backing array.
    ///         </item>
    ///         <item>
    ///             <see cref="ArgumentException" /> (and its <see cref="ArgumentNullException" /> /
    ///             <see cref="ArgumentOutOfRangeException" /> subtypes) — a length prefix beyond
    ///             <see cref="int.MaxValue" />, an <c>SCV_VEC</c>/<c>SCV_MAP</c> whose optional body is absent, a
    ///             metadata version this SDK does not model, or a decoded field rejected by the domain type it is
    ///             handed to.
    ///         </item>
    ///         <item>
    ///             <see cref="InvalidOperationException" /> — a discriminant the SDK's own <c>FromXdr</c> dispatch
    ///             does not accept. <see cref="ResultValue" /> deliberately excludes this one; see its catch clause.
    ///         </item>
    ///         <item>
    ///             <see cref="AssetCodeLengthInvalidException" /> — a <c>TRUSTLINE</c> ledger-entry change carrying
    ///             an asset code that is empty once its trailing NUL padding is stripped, or that is too short for
    ///             its <c>ALPHANUM12</c> discriminant. Four zero bytes in an otherwise well-formed blob are enough.
    ///             It derives straight from <see cref="Exception" />, so no hierarchy above covers it and it has to
    ///             be named — which is exactly why an earlier hand-rolled filter here missed it and let it escape
    ///             <see cref="TransactionMeta" />.
    ///         </item>
    ///     </list>
    ///     <para>
    ///         The list is empirical, not proven exhaustive. A payload that provokes something outside it —
    ///         including <see cref="OutOfMemoryException" /> from the unbounded allocation the generated array
    ///         decoders still permit — propagates, by design: these properties report bad input, not every
    ///         conceivable failure.
    ///     </para>
    /// </remarks>
    private static bool IsPayloadFailure(Exception exception)
    {
        return exception is FormatException or InvalidDataException or IOException
            or IndexOutOfRangeException or ArgumentException or InvalidOperationException
            or AssetCodeLengthInvalidException;
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