﻿using System;
using Newtonsoft.Json;
using StellarDotnetSdk.Soroban;

namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Holds the details of the response of <c>getEvents()</c>.
/// </summary>
public class GetEventsResponse
{
    public GetEventsResponse(EventInfo[]? events, long? latestLedger, string? cursor)
    {
        Events = events;
        LatestLedger = latestLedger;
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

    public string? Cursor { get; }

    public class EventInfo
    {
        public EventInfo(
            string contractId,
            string id,
            bool inSuccessfulContractCall,
            int ledger,
            string ledgerClosedAt,
            string pagingToken,
            string[] topics,
            string type,
            string value,
            string transactionHash)
        {
            ContractId = contractId;
            Id = id;
            InSuccessfulContractCall = inSuccessfulContractCall;
            Ledger = ledger;
            LedgerClosedAt = ledgerClosedAt;
            PagingToken = pagingToken;
            Topics = topics;
            Type = type;
            Value = value;
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
        ///     Duplicate of <c>id</c> field, but in the standard place for pagination tokens.
        /// Use <see cref="GetEventsResponse.Cursor"/> instead.
        /// </summary>
        [Obsolete("This property is deprecated, use GetEventsResponse.Cursor instead. In a future release of this SDK this field can be removed.")]
        public string PagingToken { get; }

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

        [JsonProperty("txHash")] public string TransactionHash { get; }
    }
}