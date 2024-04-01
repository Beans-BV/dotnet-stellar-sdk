using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Holds the details of the response of <c>getEvents()</c>.
/// </summary>
[JsonObject]
public class GetEventsResponse
{
    public GetEventsResponse(EventInfo[]? events, long? latestLedger)
    {
        Events = events;
        LatestLedger = latestLedger;
    }

    /// <summary>
    ///     If error is present then results will not be in the response
    /// </summary>
    [JsonProperty("events")]
    public EventInfo[]? Events { get; }

    /// <summary>
    ///     The sequence number of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    [JsonProperty("latestLedger")]
    public long? LatestLedger { get; }

    [JsonObject]
    public class EventInfo
    {
        public EventInfo(string contractId, string id, bool inSuccessfulContractCall, int ledger, string ledgerClosedAt,
            string pagingToken, string[] topics, string type, string value)
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
        }

        /// <summary>
        ///     StrKey representation of the contract address that emitted this event.
        /// </summary>
        [JsonProperty("contractId")]
        public string ContractId { get; }

        /// <summary>
        ///     Unique identifier for this event.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; }

        /// <summary>
        ///     If true the event was emitted during a successful contract call.
        /// </summary>
        [JsonProperty("inSuccessfulContractCall")]
        public bool InSuccessfulContractCall { get; }

        /// <summary>
        ///     Sequence number of the ledger in which this event was emitted.
        /// </summary>
        [JsonProperty("ledger")]
        public int Ledger { get; }

        /// <summary>
        ///     ISO-8601 timestamp of the ledger closing time.
        ///     See https://www.iso.org/iso-8601-date-and-time-format.html.
        /// </summary>
        [JsonProperty("ledgerClosedAt")]
        public string LedgerClosedAt { get; }

        /// <summary>
        ///     Duplicate of <c>id</c> field, but in the standard place for pagination tokens.
        /// </summary>
        [JsonProperty("pagingToken")]
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
        [JsonProperty("type")]
        public string Type { get; }

        /// <summary>
        ///     A base-64 encoded XDR string of an <see cref="Xdr.SCVal">xdr.SCVal</see> object represents the data the event was
        ///     broadcasting in the emitted event.
        /// </summary>
        /// <remarks>
        ///     Can be deserialized into an <see cref="SCVal" /> object by calling
        ///     <see cref="SCVal.FromXdrBase64">SCVal.FromXdrBase64()</see>.
        /// </remarks>
        [JsonProperty("value")]
        public string Value { get; }
    }
}