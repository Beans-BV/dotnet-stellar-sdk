using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Requests.SorobanRpc;

public class GetEventsRequest
{
    /// <summary>
    ///     Ledger sequence number to start fetching responses from (inclusive). This method will return an error if
    ///     startLedger is less than the oldest ledger stored in this node, or greater than the latest ledger seen by this
    ///     node. If a cursor is included in the request, startLedger must be omitted.
    /// </summary>
    [JsonPropertyName("startLedger")]
    public long? StartLedger { get; set; }

    /// <summary>
    ///     List of filters for the returned events. Events matching any of the filters are included. To match a filter, an
    ///     event must match both a contractId and a topic. Maximum 5 filters are allowed per request.
    /// </summary>
    [JsonPropertyName("filters")]
    public EventFilter[]? Filters { get; set; }

    [JsonPropertyName("pagination")]
    public PaginationOptions? Pagination { get; set; }

    public class EventFilter
    {
        /// <summary>
        ///     A comma separated list of event types (system, contract, or diagnostic) used to filter events. If omitted, all
        ///     event types are included.
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        ///     List of contract IDs to query for events. If omitted, return events for all contracts. Maximum 5 contract IDs are
        ///     allowed per request.
        /// </summary>
        [JsonPropertyName("contractIds")]
        public string[]? ContractIds { get; set; }

        /// <summary>
        ///     List of topic filters. If omitted, query for all events. If multiple filters are specified, events will be included
        ///     if they match any of the filters. Maximum 5 filters are allowed per request.
        /// </summary>
        [JsonPropertyName("topics")]
        public string[][]? Topics { get; set; }
    }

    public class PaginationOptions
    {
        [JsonPropertyName("cursor")]
        public string? Cursor;

        [JsonPropertyName("limit")]
        public long? Limit;
    }
}