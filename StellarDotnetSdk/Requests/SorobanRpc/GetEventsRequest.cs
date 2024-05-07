using Newtonsoft.Json;

namespace StellarDotnetSdk.Requests.SorobanRpc;

public class GetEventsRequest
{
    [JsonProperty(PropertyName = "startLedger")]
    public long? StartLedger { get; set; }

    [JsonProperty(PropertyName = "filters")]
    public EventFilter[]? Filters { get; set; }

    [JsonProperty(PropertyName = "pagination")]
    public PaginationOptions? Pagination { get; set; }

    public class EventFilter
    {
        [JsonProperty(PropertyName = "type")] public string? Type { get; set; }

        [JsonProperty(PropertyName = "contractIds")]
        public string[]? ContractIds { get; set; }

        [JsonProperty(PropertyName = "topics")]
        public string[][]? Topics { get; set; }
    }

    public class PaginationOptions
    {
        [JsonProperty(PropertyName = "cursor")]
        public string? Cursor;

        [JsonProperty(PropertyName = "limit")] public long? Limit;
    }
}