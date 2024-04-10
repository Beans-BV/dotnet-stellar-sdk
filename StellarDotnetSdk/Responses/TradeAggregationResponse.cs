using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class TradeAggregationResponse : Response
{
    [JsonProperty(PropertyName = "timestamp")]
    public string Timestamp { get; init; }

    [JsonProperty(PropertyName = "trade_count")]
    public string TradeCount { get; init; }

    [JsonProperty(PropertyName = "base_volume")]
    public string BaseVolume { get; init; }

    [JsonProperty(PropertyName = "counter_volume")]
    public string CounterVolume { get; init; }

    [JsonProperty(PropertyName = "avg")] public string Avg { get; init; }

    [JsonProperty(PropertyName = "high")] public string High { get; init; }

    [JsonProperty(PropertyName = "low")] public string Low { get; init; }

    [JsonProperty(PropertyName = "open")] public string Open { get; init; }

    [JsonProperty(PropertyName = "close")] public string Close { get; init; }
}