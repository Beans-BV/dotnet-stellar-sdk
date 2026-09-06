using System;
using System.IO;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;
// Deliberately NOT `using StellarDotnetSdk.Exceptions`: that namespace declares its own FormatException,
// which would silently shadow System.FormatException in the filter below. The SDK types it contributes
// here are named in full.
using AssetCodeLengthInvalidException = StellarDotnetSdk.Exceptions.AssetCodeLengthInvalidException;

namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     <para>
///         The response will include the anticipated affects the given transaction will have on the network. Additionally,
///         information needed to build, sign, and actually submit the transaction will be provided.
///     </para>
///     See https://developers.stellar.org/docs/data/apis/rpc/api-reference/methods/simulateTransaction/
/// </summary>
public class SimulateTransactionResponse
{
    [JsonInclude]
    private string? TransactionData { get; init; }

    /// <summary>
    ///     This field will include details about why the invoke host function call failed.
    /// </summary>
    /// <para>(optional) Only present if the transaction failed.</para>
    public string? Error { get; init; }

    /// <summary>
    ///     Array of serialized base64 strings - Array of the events emitted during the contract invocation. The events are
    ///     ordered by their emission time. (an array of serialized base64 strings).
    ///     <para>
    ///         Only present when simulating of InvokeHostFunction operations, note that it can be present on error,
    ///         providing extra context about what failed.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     A <c>null</c> element is rejected, for the reason given on <see cref="StateChanges" />: the
    ///     non-nullable element type is otherwise unenforced against the array's <em>contents</em>, and
    ///     <c>"events":[null]</c> handed the caller an array whose first <c>Length</c> read threw
    ///     <see cref="NullReferenceException" />.
    /// </remarks>
    /// <exception cref="System.Text.Json.JsonException">
    ///     Thrown during deserialization when the array contains a <c>null</c> element.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when a <c>null</c> element is assigned from C# rather than read from JSON.
    /// </exception>
    [JsonConverter(typeof(SimulationEventsArrayJsonConverter))]
    public string[]? Events
    {
        get => _events;
        init
        {
            ThrowIfAnyElementIsNull(value, nameof(Events));
            _events = value;
        }
    }

    private readonly string[]? _events;

    /// <summary>
    ///     The sequence number of the latest ledger known to Stellar RPC at the time it handled the request.
    /// </summary>
    public long? LatestLedger { get; init; }

    /// <summary>
    ///     (Optional) Not present in case of error.
    ///     <para>
    ///         Recommended minimum resource fee (in stroops) to add when submitting the transaction. This fee is to be added
    ///         on top of the Stellar network fee.
    ///     </para>
    ///     <para>
    ///         Stellar RPC declares this field as an <c>int64</c>, so it is modelled as a <see cref="long" />: values above
    ///         <see cref="uint.MaxValue" /> (~429 XLM) are reachable on large uploads and restores.
    ///     </para>
    ///     See https://developers.stellar.org/docs/encyclopedia/fees-surge-pricing-fee-strategies#network-fees-on-stellar.
    /// </summary>
    public long? MinResourceFee { get; init; }

    /// <summary>
    ///     If present, it indicates that the simulation detected archived ledger entries which need to be restored before the
    ///     submission of the <c>InvokeHostFunction</c> operation. The <see cref="MinResourceFee" /> and
    ///     <see cref="SorobanTransactionData" /> fields should be used to submit a transaction containing a
    ///     <c>RestoreFootprint</c> operation.
    ///     <para>
    ///         (optional) It can only be present on successful simulation (i.e. no error) of <c>InvokeHostFunction</c>
    ///         operations.
    ///     </para>
    /// </summary>
    [JsonPropertyName("restorePreamble")]
    public RestorePreamble? RestorePreambleInfo { get; init; }

    /// <summary>
    ///     (optional) An array of state changes that would result from executing the simulated transaction.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A <c>null</c> <em>element</em> is rejected. <c>RespectNullableAnnotations</c> constrains the
    ///         array reference, never its contents, so <c>"stateChanges":[null]</c> otherwise produced an array
    ///         whose element was <see langword="null" /> despite the non-nullable element type — and the very
    ///         loop <see cref="LedgerEntryChange.Type" />'s <see cref="JsonRequiredAttribute" /> was added to
    ///         protect, <c>foreach (var c in StateChanges) if (c.Type == "created")</c>, then threw
    ///         <see cref="NullReferenceException" /> instead of the <see cref="System.Text.Json.JsonException" />
    ///         every other malformed-payload path here reports.
    ///     </para>
    ///     <para>
    ///         The two entry points report differently, because they are different kinds of failure: a
    ///         malformed payload is a <see cref="System.Text.Json.JsonException" /> raised by
    ///         <see cref="NonNullElementArrayJsonConverter{T}" /> before the value is assigned, while passing a
    ///         null element from ordinary C# is a rejected <em>argument</em> and raises
    ///         <see cref="ArgumentException" /> — matching the convention the SDK's request types follow, and
    ///         keeping a caller's <c>catch (JsonException)</c> from swallowing a programming error.
    ///     </para>
    ///     <para>
    ///         This is a check at assignment, not a maintained invariant: the array is stored and returned by
    ///         reference rather than copied, so anything still holding the array can write a null back into it
    ///         afterwards. It guards what arrives from the wire, which is where a null element actually comes
    ///         from.
    ///     </para>
    /// </remarks>
    /// <exception cref="System.Text.Json.JsonException">
    ///     Thrown during deserialization when the array contains a <c>null</c> element.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when a <c>null</c> element is assigned from C# rather than read from JSON.
    /// </exception>
    [JsonPropertyName("stateChanges")]
    [JsonConverter(typeof(StateChangesArrayJsonConverter))]
    public LedgerEntryChange[]? StateChanges
    {
        get => _stateChanges;
        init
        {
            ThrowIfAnyElementIsNull(value, nameof(StateChanges));
            _stateChanges = value;
        }
    }

    private readonly LedgerEntryChange[]? _stateChanges;

    /// <summary>
    ///     Rejects a <c>null</c> element assigned from C#. The deserialization path never reaches this: the
    ///     property's <see cref="NonNullElementArrayJsonConverter{T}" /> rejects the payload first, with a
    ///     <see cref="System.Text.Json.JsonException" /> as every other malformed-payload path here does.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when any element is <c>null</c>.</exception>
    private static void ThrowIfAnyElementIsNull<T>(T[]? value, string parameterName) where T : class
    {
        if (value == null)
        {
            return;
        }

        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] is null)
            {
                throw new ArgumentException($"The array contains a null element at index {i}.", parameterName);
            }
        }
    }

    /// <summary>
    ///     An array of the individual host function call results.
    ///     This will only contain a single element if present, because only a single
    ///     <c>invokeHostFunctionOperation</c> is supported per transaction.
    /// </summary>
    /// <remarks>
    ///     A <c>null</c> element is rejected, for the reason given on <see cref="StateChanges" />.
    ///     <c>"results":[null]</c> previously deserialized cleanly and threw
    ///     <see cref="NullReferenceException" /> at the caller on the first <c>Results[0].Xdr</c>; it also made
    ///     <see cref="SorobanAuthorization" /> answer <see langword="null" />, which reads as <em>no
    ///     authorization required</em> and is how the documented assemble-and-submit flow ends up sending a
    ///     transaction with no auth entries.
    /// </remarks>
    /// <exception cref="System.Text.Json.JsonException">
    ///     Thrown during deserialization when the array contains a <c>null</c> element.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when a <c>null</c> element is assigned from C# rather than read from JSON.
    /// </exception>
    [JsonConverter(typeof(SimulationResultsArrayJsonConverter))]
    public SimulateInvokeHostFunctionResult[]? Results
    {
        get => _results;
        init
        {
            ThrowIfAnyElementIsNull(value, nameof(Results));
            _results = value;
        }
    }

    private readonly SimulateInvokeHostFunctionResult[]? _results;

    /// <summary>
    ///     The recommended Soroban Transaction Data to use when submitting the simulated transaction. This data contains the
    ///     refundable fee and resource usage information such as the ledger footprint and IO access data.
    ///     <para>Not present in case of error.</para>
    ///     <para>
    ///         The blob is decoded from server-supplied base64 on every read, so this property can throw and each
    ///         read returns a fresh object graph. A <c>SorobanTransactionData != null</c> guard is therefore not a
    ///         safe way to probe for presence, and reading the property twice pays for two decodes — bind it to a
    ///         local instead.
    ///     </para>
    /// </summary>
    /// <exception cref="InvalidDataException">
    ///     Thrown when the server-supplied <c>transactionData</c> is not decodable as a
    ///     <c>SorobanTransactionData</c> XDR blob. Decoding happens on every read of this property, not during
    ///     deserialization, so the failure surfaces here rather than at the originating
    ///     <see cref="StellarRpcServer.SimulateTransaction" /> call. This is the failure this property reports for
    ///     a malformed blob; it is not a guarantee that no other exception can escape (see
    ///     <c>IsXdrDecodeFailure</c>).
    /// </exception>
    [JsonIgnore]
    public SorobanTransactionData? SorobanTransactionData
    {
        get
        {
            if (TransactionData == null)
            {
                return null;
            }
            try
            {
                return SorobanTransactionData.FromXdrBase64(TransactionData);
            }
            catch (Exception ex) when (IsXdrDecodeFailure(ex))
            {
                throw new InvalidDataException("Malformed Soroban transaction data XDR: " + ex.Message, ex);
            }
        }
    }

    /// <summary>
    ///     (optional) Array of Soroban authorization entries required for the simulated transaction.
    ///     Derived from the first result's auth entries.
    ///     <para>
    ///         The entries are decoded from server-supplied base64 on every read, so this property can throw and
    ///         each read returns a fresh object graph. A <c>SorobanAuthorization != null</c> guard is therefore not
    ///         a safe way to probe for their presence; check <c>Results?[0].Auth</c>, or handle
    ///         <see cref="InvalidDataException" />. Bind the value to a local rather than reading the property
    ///         twice, which decodes every entry twice.
    ///     </para>
    /// </summary>
    /// <exception cref="InvalidDataException">
    ///     Thrown when any of the server-supplied auth entries is not decodable as a
    ///     <c>SorobanAuthorizationEntry</c> XDR blob — including an unknown <c>SorobanCredentialsType</c>
    ///     discriminant. The originating decoder exception is preserved as the inner exception. This is the failure
    ///     this property reports for a malformed blob; it is not a guarantee that no other exception can escape
    ///     (see <c>IsXdrDecodeFailure</c>).
    /// </exception>
    // [JsonIgnore] for the same reason SorobanTransactionData carries it, plus one this property adds:
    // serialization reads every property, so without it JsonSerializer.Serialize(response) invokes this
    // getter and an InvalidDataException escapes from inside Serialize — for a response the caller may only
    // be trying to log or cache. The value is derived from Results[0].Auth, which is serialized already, so
    // nothing is lost from the payload.
    [JsonIgnore]
    public SorobanAuthorizationEntry[]? SorobanAuthorization
    {
        get
        {
            // `Results[0] == null` is no longer reachable from a deserialized response — the property's
            // NonNullElementArrayJsonConverter rejects `"results": [null]` at the point it arrives, which is
            // better than answering it here: returning null for a malformed payload is indistinguishable
            // from "this simulation needs no authorization", and the documented assemble-and-submit flow
            // guards on exactly that. The check stays because the array is not copied on assignment, so a
            // caller holding the reference can still write a null into it afterwards.
            if (Results is not { Length: > 0 } || Results[0] == null)
            {
                return null;
            }
            var auth = Results[0].Auth;
            if (auth == null)
            {
                return null;
            }

            var entries = new SorobanAuthorizationEntry[auth.Length];
            for (var i = 0; i < auth.Length; i++)
            {
                try
                {
                    entries[i] = SorobanAuthorizationEntry.FromXdrBase64(auth[i]);
                }
                catch (Exception ex) when (IsXdrDecodeFailure(ex))
                {
                    throw new InvalidDataException(
                        $"Malformed authorization entry XDR at index {i}: {ex.Message}", ex);
                }
            }
            return entries;
        }
    }

    /// <summary>
    ///     Recognizes the exceptions that decoding an attacker- or server-controlled base64 XDR blob can produce, so
    ///     that they can be normalized to the single <see cref="InvalidDataException" /> these properties document.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The set is broader than the one <c>Sep45Challenge</c> uses, because that method decodes with the
    ///         generated <c>Xdr.SorobanAuthorizationEntry.Decode</c> and stops there, whereas these properties go
    ///         on through the SDK's own <c>FromXdr</c> dispatch (<c>SCVal</c>, <c>ScAddress</c>, <c>Asset</c>,
    ///         <c>TrustlineAsset</c>, …), which raises its own argument- and state-validation exceptions.
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="InvalidDataException" /> — an unknown enum discriminant, an over-large element count,
    ///             or a fixed-width read past the end of the buffer, raised by the generated XDR decoders.
    ///         </item>
    ///         <item>
    ///             <see cref="IOException" /> (and <see cref="EndOfStreamException" />) — truncated input, or
    ///             non-zero opaque padding.
    ///         </item>
    ///         <item><see cref="FormatException" /> — invalid base64, or a length prefix that runs off the buffer.</item>
    ///         <item><see cref="IndexOutOfRangeException" /> — a read past the end of the backing array.</item>
    ///         <item>
    ///             <see cref="ArgumentException" /> (and its <see cref="ArgumentNullException" /> /
    ///             <see cref="ArgumentOutOfRangeException" /> subtypes) — a null entry, a length prefix beyond
    ///             <see cref="int.MaxValue" />, an <c>SCV_VEC</c>/<c>SCV_MAP</c> whose optional body is absent, or a
    ///             decoded field rejected by the domain type it is handed to.
    ///         </item>
    ///         <item>
    ///             <see cref="InvalidOperationException" /> — a discriminant the SDK's own <c>FromXdr</c> dispatch
    ///             does not accept, e.g. an <c>SCAddress</c> that decodes but is not a legal
    ///             <c>invokeHostFunction</c> argument. (Not <c>SorobanCredentials.FromXdr</c>: the generated
    ///             <c>SorobanCredentialsType.Decode</c> rejects an unknown discriminant first, with
    ///             <see cref="InvalidDataException" />.)
    ///         </item>
    ///         <item>
    ///             <see cref="AssetCodeLengthInvalidException" /> — a footprint <c>TRUSTLINE</c> key, or a
    ///             <c>createContract</c> argument, carrying an asset code that is empty once its trailing NUL
    ///             padding is stripped, or that is too short for its <c>ALPHANUM12</c> discriminant. Four zero
    ///             bytes in an otherwise well-formed blob are enough. It derives straight from
    ///             <see cref="Exception" />, so no hierarchy above covers it and it has to be named.
    ///         </item>
    ///     </list>
    ///     <para>
    ///         Note that the SDK declares a <c>FormatException</c> of its own in
    ///         <c>StellarDotnetSdk.Exceptions</c>. Importing that namespace into this file would shadow
    ///         <see cref="FormatException" /> in the filter below and silently stop catching the invalid-base64
    ///         failures it exists for, so the SDK exception type above is aliased in rather than imported.
    ///     </para>
    ///     <para>
    ///         The list is empirical, not proven exhaustive: it covers every exception type observed across a fuzz
    ///         of the real decoders plus a sweep of the SDK exception types reachable from these two roots. A
    ///         response that provokes something outside it — including <see cref="OutOfMemoryException" /> from the
    ///         unbounded allocation the generated array decoders still permit — will propagate unnormalized. Treat
    ///         <see cref="InvalidDataException" /> as the failure this API reports, not as a guarantee that nothing
    ///         else can escape.
    ///     </para>
    /// </remarks>
    private static bool IsXdrDecodeFailure(Exception ex)
    {
        return ex is InvalidDataException or IOException or FormatException or IndexOutOfRangeException
            or ArgumentException or InvalidOperationException or AssetCodeLengthInvalidException;
    }

    /// <summary>
    ///     It can only present on successful simulation (i.e. no error) of <c>InvokeHostFunction</c> operations.
    ///     If present, it indicates the simulation detected expired ledger entries which requires restoring
    ///     with the submission of a <c>RestoreFootprint</c> operation before submitting the <c>InvokeHostFunction</c>
    ///     operation.
    ///     The <c>MinResourceFee</c> and <c>SorobanTransactionData</c> fields should be used to construct the transaction
    ///     containing the
    ///     <c>RestoreFootprint</c> operation.
    /// </summary>
    public class RestorePreamble
    {
        [JsonInclude]
        private string? TransactionData { get; init; }

        /// <summary>
        ///     Recommended minimum resource fee to add when submitting the <c>RestoreFootprint</c> operation. This fee is to be
        ///     added on
        ///     top of the Stellar network fee.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <see cref="JsonRequiredAttribute" /> stops a preamble that omits the field entirely from
        ///         reading as a free restore. This is a value type, so nothing else enforces presence: an object
        ///         with no <c>minResourceFee</c> silently yielded <c>0</c>, and a caller following the documented
        ///         "use <see cref="MinResourceFee" /> and <see cref="SorobanTransactionData" /> to submit a
        ///         RestoreFootprint operation" flow would then submit it underfunded. Stellar RPC declares the
        ///         field <c>json:"minResourceFee,string"</c> with no <c>omitempty</c>, so a conforming server
        ///         always sends it and requiring it rejects nothing.
        ///     </para>
        ///     <para>
        ///         This enforces <em>presence only</em>, not plausibility: an explicit <c>"0"</c> or a negative
        ///         <c>"-500"</c> both satisfy the attribute and still describe a restore this SDK would submit
        ///         underfunded. Nothing here range-checks the value, and <see cref="SorobanTransactionData" />
        ///         — the other half of the documented flow — carries no presence requirement at all, so a
        ///         preamble consisting of nothing but a fee still deserializes. Callers who act on a preamble
        ///         should check both fields rather than assume deserialization vouched for them.
        ///     </para>
        ///     <para>
        ///         <c>required</c> carries the guarantee to the other entry point, for the reason given on
        ///         <see cref="LedgerEntryChange.Type" />: the attribute binds only the deserializer, so without it
        ///         <c>new RestorePreamble()</c> still produced the silent <c>0</c> this remark opens by
        ///         describing.
        ///     </para>
        /// </remarks>
        [JsonRequired]
        public required long MinResourceFee { get; init; }

        /// <summary>
        ///     The recommended Soroban Transaction Data to use when submitting the <c>RestoreFootprint</c> operation.
        ///     <para>Null if the preamble carried no transaction data.</para>
        /// </summary>
        /// <exception cref="InvalidDataException">
        ///     Thrown when the server-supplied <c>transactionData</c> is not decodable as a
        ///     <c>SorobanTransactionData</c> XDR blob, matching
        ///     <see cref="SimulateTransactionResponse.SorobanTransactionData" />. Decoding happens on every read of
        ///     this property, not during deserialization.
        /// </exception>
        [JsonIgnore]
        public SorobanTransactionData? SorobanTransactionData
        {
            get
            {
                if (TransactionData == null)
                {
                    return null;
                }
                try
                {
                    return SorobanTransactionData.FromXdrBase64(TransactionData);
                }
                catch (Exception ex) when (IsXdrDecodeFailure(ex))
                {
                    throw new InvalidDataException(
                        "Malformed restore preamble Soroban transaction data XDR: " + ex.Message, ex);
                }
            }
        }
    }

    /// <summary>
    ///     Information about the fees expected, instructions used, etc.
    /// </summary>
    public class SimulateTransactionCost
    {
        /// <summary>
        ///     Number of the total cpu instructions consumed by this transaction.
        /// </summary>
        [JsonPropertyName("cpuInsns")]
        public long CpuInstructions { get; init; }

        /// <summary>
        ///     Number of the total memory bytes allocated by this transaction.
        /// </summary>
        [JsonPropertyName("memBytes")]
        public long MemoryBytes { get; init; }
    }

    /// <summary>
    ///     Used as a part of simulate transaction.
    ///     See https://developers.stellar.org/docs/data/apis/rpc/api-reference/methods/simulateTransaction
    /// </summary>
    public class SimulateInvokeHostFunctionResult
    {
        /// <summary>
        ///     Array of serialized base64 strings - Per-address authorizations recorded when simulating this Host Function call.
        /// </summary>
        /// <remarks>
        ///     A <c>null</c> element is rejected, for the reason given on
        ///     <see cref="SimulateTransactionResponse.StateChanges" />. Without the guard a null element was only
        ///     <em>incidentally</em> contained: <see cref="SimulateTransactionResponse.SorobanAuthorization" />
        ///     fed it to the base64 decoder, whose <see cref="ArgumentNullException" /> the decode-failure filter
        ///     happened to catch and report as a malformed XDR blob — which is not what went wrong, and is not a
        ///     guarantee for a caller reading <c>Results[i].Auth</c> directly.
        /// </remarks>
        /// <exception cref="System.Text.Json.JsonException">
        ///     Thrown during deserialization when the array contains a <c>null</c> element.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when a <c>null</c> element is assigned from C# rather than read from JSON.
        /// </exception>
        [JsonConverter(typeof(SorobanAuthArrayJsonConverter))]
        public string[]? Auth
        {
            get => _auth;
            init
            {
                ThrowIfAnyElementIsNull(value, nameof(Auth));
                _auth = value;
            }
        }

        private readonly string[]? _auth;

        /// <summary>
        ///     (optional) Only present on success. xdr-encoded return value of the contract call operation.
        /// </summary>
        public string? Xdr { get; init; } // TODO Unit test on error
    }

    /// <summary>
    ///     Represents a change to a ledger entry that would result from executing the simulated transaction,
    ///     including the entry's state before and after the change.
    /// </summary>
    public class LedgerEntryChange
    {
        /// <summary>
        ///     The type of ledger entry change: <c>"created"</c>, <c>"updated"</c> or <c>"deleted"</c>.
        ///     <para>
        ///         Non-nullable, unlike <see cref="Key" />: Stellar RPC tags this field neither <c>omitempty</c> nor
        ///         optional and its <c>MarshalJSON</c> always writes a string, so a conforming server always sends
        ///         it.
        ///     </para>
        ///     <para>
        ///         An empty string is possible and is not one of the three documented types: Stellar RPC v23.0.0 and
        ///         v23.0.1 emitted pre-allocated no-op state changes whose <c>type</c> marshalled to <c>""</c>
        ///         (fixed in v23.0.2, stellar/stellar-rpc#506) — the same entries that omitted <see cref="Key" />.
        ///         Compare against the three literals rather than assuming a non-empty value.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <see cref="JsonRequiredAttribute" /> makes "a conforming server always sends it" a contract the
        ///         deserializer enforces, matching <see cref="SendTransactionResponse.Hash" />. Without it the
        ///         non-nullable annotation was unenforced in exactly the direction that matters:
        ///         <c>RespectNullableAnnotations</c> rejects an explicit <c>null</c> but not an <em>absent</em>
        ///         property, so an entry with no <c>type</c> left this <see cref="string" /> holding
        ///         <see langword="null" /> and every <c>Type == "created"</c> comparison silently returned false.
        ///         Requiring it rejects nothing a conforming server sends. Note this enforces presence, not
        ///         membership: the empty string described above still deserializes.
        ///     </para>
        ///     <para>
        ///         <c>required</c> carries the same guarantee to the other entry point. The attribute only binds
        ///         the deserializer, so without it <c>new LedgerEntryChange { Before = "…" }</c> still compiled
        ///         and still left a non-nullable <see cref="string" /> holding <see langword="null" /> — the very
        ///         state the attribute was added to make unreachable, one entry point over. This matches how the
        ///         array properties on this type guard both paths, and how <c>Claimant</c> and
        ///         <c>ClaimableBalanceResponse</c> already declare their always-present fields.
        ///     </para>
        /// </remarks>
        [JsonRequired]
        public required string Type { get; init; }

        /// <summary>
        ///     The base64-encoded XDR key of the affected ledger entry.
        ///     <para>
        ///         (optional) Stellar RPC tags this field <c>omitempty</c>, so it is absent whenever the key is
        ///         empty. That is not hypothetical: v23.0.0 and v23.0.1 shipped pre-allocated no-op state changes
        ///         with no key at all — the same entries whose <see cref="Type" /> marshalled to <c>""</c> — fixed
        ///         in v23.0.2 (stellar/stellar-rpc#506). It is also absent when the entry's key travels in the
        ///         JSON-XDR <c>keyJson</c> field instead, though this SDK never requests that format.
        ///     </para>
        ///     <para>
        ///         A missing property does not violate a non-nullable annotation the way an explicit <c>null</c>
        ///         does, so before this was widened the property advertised a guarantee the wire did not honour and
        ///         the compiler suppressed exactly the null checks that would have caught it.
        ///     </para>
        /// </summary>
        public string? Key { get; init; }

        /// <summary>
        ///     The base64-encoded XDR of the ledger entry before the change, or null for created entries.
        /// </summary>
        public string? Before { get; init; }

        /// <summary>
        ///     The base64-encoded XDR of the ledger entry after the change, or null for deleted entries.
        /// </summary>
        public string? After { get; init; }
    }
}