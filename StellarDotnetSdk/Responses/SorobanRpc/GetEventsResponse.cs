using System;
using Newtonsoft.Json;
using StellarDotnetSdk.Soroban;

namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Holds the details of the response of <c>getEvents()</c>.
/// </summary>
public class GetEventsResponse
{
    public GetEventsResponse(
        EventInfo[]? events,
        long? latestLedger,
        long? oldestLedger,
        long? latestLedgerCloseTime,
        long? oldestLedgerCloseTime,
        string? cursor
    )
    {
        Events = events;
        LatestLedger = latestLedger;
        OldestLedger = oldestLedger;
        LatestLedgerCloseTime = latestLedgerCloseTime;
        OldestLedgerCloseTime = oldestLedgerCloseTime;
        Cursor = cursor;
    }

    /// <summary>
    ///     If error is present then results will not be in the response
    /// </summary>
    public EventInfo[]? Events { get; }

    /// <summary>
    ///     The sequence number of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    public long? LatestLedger { get; }

    public long? OldestLedger { get; }
    public long? LatestLedgerCloseTime { get; }
    public long? OldestLedgerCloseTime { get; }
    public string? Cursor { get; }

    public class EventInfo
    {
        public EventInfo(
            string contractId,
            string id,
            bool inSuccessfulContractCall,
            int ledger,
            string ledgerClosedAt,
            string[] topics,
            string type,
            string value,
            uint transactionIndex,
            uint operationIndex,
            string transactionHash)
        {
            ContractId = contractId;
            Id = id;
            InSuccessfulContractCall = inSuccessfulContractCall;
            Ledger = ledger;
            LedgerClosedAt = ledgerClosedAt;
            Topics = topics;
            Type = type;
            Value = value;
            TransactionIndex = transactionIndex;
            OperationIndex = operationIndex;
            TransactionHash = transactionHash;
        }

        /// <summary>
        ///     StrKey representation of the contract address that emitted this event.
        /// </summary>
        public string ContractId { get; }

        /// <summary>
        ///     Unique identifier for this event.
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     If true the event was emitted during a successful contract call.
        /// </summary>
        [Obsolete("Deprecated. Will be removed in the next version.")]
        public bool InSuccessfulContractCall { get; }

        /// <summary>
        ///     Sequence number of the ledger in which this event was emitted.
        /// </summary>
        public int Ledger { get; }

        /// <summary>
        ///     ISO-8601 timestamp of the ledger closing time.
        ///     See https://www.iso.org/iso-8601-date-and-time-format.html.
        /// </summary>
        public string LedgerClosedAt { get; }

        /// <summary>
        ///     A list containing the topics, each is a base-64 encoded XDR string of an <see cref="Xdr.SCVal">xdr.SCVal</see>
        ///     object, this event was emitted with.
        /// </summary>
        /// ///
        /// <remarks>
        ///     Can be deserialized into an <see cref="SCVal" /> object by calling
        ///     <see cref="SCVal.FromXdrBase64">SCVal.FromXdrBase64()</see>.
        /// </remarks>
        [JsonProperty("topic")]
        public string[] Topics { get; }

        /// <summary>
        ///     The type of event emission. Allowed values: contract, diagnostic, system.
        /// </summary>
        public string Type { get; }

        /// <summary>
        ///     A base-64 encoded XDR string of an <see cref="Xdr.SCVal">xdr.SCVal</see> object represents the data the event was
        ///     broadcasting in the emitted event.
        /// </summary>
        /// <remarks>
        ///     Can be deserialized into an <see cref="SCVal" /> object by calling
        ///     <see cref="SCVal.FromXdrBase64">SCVal.FromXdrBase64()</see>.
        /// </remarks>
        public string Value { get; }

        public uint TransactionIndex { get; }
        public uint OperationIndex { get; }
        [JsonProperty("txHash")] public string TransactionHash { get; }
    }
}