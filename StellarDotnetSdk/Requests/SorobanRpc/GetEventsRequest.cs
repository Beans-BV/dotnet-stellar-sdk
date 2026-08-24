using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Requests.SorobanRpc;

/// <summary>
///     Represents the request parameters for the Soroban RPC <c>getEvents</c> method,
///     which retrieves contract events from the network.
/// </summary>
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

    /// <summary>
    ///     Pagination options for controlling the cursor position and page size of event results.
    /// </summary>
    [JsonPropertyName("pagination")]
    public PaginationOptions? Pagination { get; set; }

    /// <summary>
    ///     Defines criteria for filtering contract events by type, contract ID, and topic.
    /// </summary>
    public class EventFilter
    {
        private EventFilterType? _type;

        /// <summary>
        ///     The event types to return, combined as flags — for example
        ///     <c>EventFilterType.System | EventFilterType.Contract</c>. If omitted (left <see langword="null" />), all
        ///     event types are included.
        ///     <para>
        ///         Note that <c>diagnostic</c> is not a selectable type: Protocol 23 removed diagnostic events from the
        ///         <c>getEvents</c> stream, and Stellar RPC v23.0.0 onwards rejects a filter that names it. See
        ///         <see cref="EventFilterType" />.
        ///     </para>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the assigned value contains bits that are not defined <see cref="EventFilterType" /> flags,
        ///     so that a bad cast such as <c>(EventFilterType)99</c> fails at the assignment rather than as a server
        ///     round-trip error.
        /// </exception>
        // The property-level [JsonConverter] is what actually pins the wire format here. System.Text.Json resolves
        // converters property attribute first, then the options' Converters collection, then the type attribute —
        // so the JsonStringEnumConverter registered on JsonOptions.DefaultOptions outranks the attribute on
        // EventFilterType and would emit "System, Contract", which Stellar RPC rejects. Only the property
        // attribute is immune to that, whichever options instance the request is serialized with.
        [JsonPropertyName("type")]
        [JsonConverter(typeof(EventFilterTypeJsonConverter))]
        public EventFilterType? Type
        {
            get => _type;
            set
            {
                if (value.HasValue && !value.Value.IsDefined())
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown event filter type.");
                }
                _type = value;
            }
        }

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

    /// <summary>
    ///     Pagination parameters for controlling the cursor position and page size of event results.
    /// </summary>
    public class PaginationOptions
    {
        /// <summary>
        ///     A cursor value for pagination. When provided, returns results after this cursor.
        /// </summary>
        [JsonPropertyName("cursor")]
        public string? Cursor { get; set; }

        /// <summary>
        ///     The maximum number of events to return in a single response.
        /// </summary>
        [JsonPropertyName("limit")]
        public long? Limit { get; set; }
    }
}