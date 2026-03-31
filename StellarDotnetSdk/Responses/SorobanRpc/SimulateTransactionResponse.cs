using System.Linq;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;

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
    public string[]? Events { get; init; }

    /// <summary>
    ///     The sequence number of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    public long? LatestLedger { get; init; }

    /// <summary>
    ///     (Optional) Not present in case of error.
    ///     <para>
    ///         Recommended minimum resource fee to add when submitting the transaction. This fee is to be added on top of the
    ///         Stellar network fee.
    ///     </para>
    ///     See https://developers.stellar.org/docs/encyclopedia/fees-surge-pricing-fee-strategies#network-fees-on-stellar.
    /// </summary>
    public uint? MinResourceFee { get; init; }

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
    public RestorePreamble? RestorePreambleInfo { get; init; } // TODO Unit test

    /// <summary>
    ///     (optional) An array of state changes that would result from executing the simulated transaction.
    /// </summary>
    [JsonPropertyName("stateChanges")]
    public LedgerEntryChange[]? StateChanges { get; init; }

    /// <summary>
    ///     An array of the individual host function call results.
    ///     This will only contain a single element if present, because only a single
    ///     <c>invokeHostFunctionOperation</c> is supported per transaction.
    /// </summary>
    public SimulateInvokeHostFunctionResult[]? Results { get; init; }

    /// <summary>
    ///     The recommended Soroban Transaction Data to use when submitting the simulated transaction. This data contains the
    ///     refundable fee and resource usage information such as the ledger footprint and IO access data.
    ///     <para>Not present in case of error.</para>
    /// </summary>
    [JsonIgnore]
    public SorobanTransactionData? SorobanTransactionData =>
        TransactionData != null ? SorobanTransactionData.FromXdrBase64(TransactionData) : null;


    /// <summary>
    ///     (optional) Array of Soroban authorization entries required for the simulated transaction.
    ///     Derived from the first result's auth entries.
    /// </summary>
    public SorobanAuthorizationEntry[]? SorobanAuthorization
    {
        get
        {
            if (Results is not { Length: > 0 } || Results[0].Auth == null)
            {
                return null;
            }
            return Results[0].Auth?.Select(SorobanAuthorizationEntry.FromXdrBase64).ToArray();
        }
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
        private string TransactionData { get; }

        /// <summary>
        ///     Recommended minimum resource fee to add when submitting the <c>RestoreFootprint</c> operation. This fee is to be
        ///     added on
        ///     top of the Stellar network fee.
        /// </summary>
        public long MinResourceFee { get; init; }

        /// <summary>
        ///     The recommended Soroban Transaction Data to use when submitting the <c>RestoreFootprint</c> operation.
        /// </summary>
        public SorobanTransactionData SorobanTransactionData =>
            SorobanTransactionData.FromXdrBase64(TransactionData); // TODO Unit test
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
        public string[]? Auth { get; init; }

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
        ///     The type of ledger entry change (e.g., "created", "updated", "deleted").
        /// </summary>
        public string Type { get; init; }

        /// <summary>
        ///     The base64-encoded XDR key of the affected ledger entry.
        /// </summary>
        public string Key { get; init; }

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