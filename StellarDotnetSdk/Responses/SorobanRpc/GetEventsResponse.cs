using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Soroban;

namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Holds the details of the response of <c>getEvents()</c>.
/// </summary>
public class GetEventsResponse
{
    /// <summary>
    ///     If error is present then results will not be in the response
    /// </summary>
    [JsonPropertyName("events")]
    public EventInfo[]? Events { get; init; }

    /// <summary>
    ///     The sequence number of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    [JsonPropertyName("latestLedger")]
    public long? LatestLedger { get; init; }

    [JsonPropertyName("oldestLedger")]
    public long? OldestLedger { get; init; }

    [JsonPropertyName("latestLedgerCloseTime")]
    public long? LatestLedgerCloseTime { get; init; }

    [JsonPropertyName("oldestLedgerCloseTime")]
    public long? OldestLedgerCloseTime { get; init; }

    [JsonPropertyName("cursor")]
    public string? Cursor { get; init; }

    public class EventInfo
    {
        /// <summary>
        ///     StrKey representation of the contract address (C...) that emitted this event.
        /// </summary>
        [JsonPropertyName("contractId")]
        public string ContractId { get; init; }

        /// <summary>
        ///     Unique identifier for this event.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; init; }

        /// <summary>
        ///     If true the event was emitted during a successful contract call.
        /// </summary>
        [Obsolete("Deprecated. Will be removed in the next version.")]
        [JsonPropertyName("inSuccessfulContractCall")]
        public bool InSuccessfulContractCall { get; init; }

        /// <summary>
        ///     Sequence number of the ledger in which this event was emitted.
        /// </summary>
        [JsonPropertyName("ledger")]
        public long Ledger { get; init; }

        /// <summary>
        ///     The time when the ledger was closed.
        /// </summary>
        [JsonPropertyName("ledgerClosedAt")]
        public DateTimeOffset LedgerClosedAt { get; init; }

        /// <summary>
        ///     A list containing the topics, each is a base-64 encoded XDR string of an <see cref="Xdr.SCVal">xdr.SCVal</see>
        ///     object, this event was emitted with.
        /// </summary>
        /// ///
        /// <remarks>
        ///     Can be deserialized into an <see cref="SCVal" /> object by calling
        ///     <see cref="SCVal.FromXdrBase64">SCVal.FromXdrBase64()</see>.
        /// </remarks>
        [JsonPropertyName("topic")]
        public string[] Topics { get; init; }

        /// <summary>
        ///     The type of event emission. Allowed values: contract, diagnostic, system.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; init; }

        /// <summary>
        ///     A base-64 encoded XDR string of an <see cref="Xdr.SCVal">xdr.SCVal</see> object represents the data the event was
        ///     broadcasting in the emitted event.
        /// </summary>
        /// <remarks>
        ///     Can be deserialized into an <see cref="SCVal" /> object by calling
        ///     <see cref="SCVal.FromXdrBase64">SCVal.FromXdrBase64()</see>.
        /// </remarks>
        [JsonPropertyName("value")]
        public string Value { get; init; }

        /// <summary>
        ///     Representing the transaction index at which the event occurred.
        /// </summary>
        [JsonPropertyName("transactionIndex")]
        public uint TransactionIndex { get; init; }

        /// <summary>
        ///     Representing the operation index at which the event occurred.
        /// </summary>
        [JsonPropertyName("operationIndex")]
        public uint OperationIndex { get; init; }

        [JsonPropertyName("txHash")]
        public string TransactionHash { get; init; }
    }
}