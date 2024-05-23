using System.Text.Json;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Requests.SorobanRpc;

public class GetEventsRequest
{
    [JsonPropertyName("startLedger")]
    public long? StartLedger { get; set; }

    [JsonPropertyName("filters")]
    public EventFilter[]? Filters { get; set; }

    [JsonPropertyName("pagination")]
    public PaginationOptions? Pagination { get; set; }

    public class EventFilter
    {
        [JsonPropertyName("type")] public string? Type { get; set; }

        [JsonPropertyName("contractIds")]
        public string[]? ContractIds { get; set; }

        [JsonPropertyName("topics")]
        public string[][]? Topics { get; set; }
    }

    public class PaginationOptions
    {
        [JsonPropertyName("cursor")]
        public string? Cursor;

        [JsonPropertyName("limit")] public long? Limit;
    }
}