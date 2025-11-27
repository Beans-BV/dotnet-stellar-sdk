using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class TradeAggregationResponse : Response
{
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; init; }

    [JsonPropertyName("trade_count")]
    public string TradeCount { get; init; }

    [JsonPropertyName("base_volume")]
    public string BaseVolume { get; init; }

    [JsonPropertyName("counter_volume")]
    public string CounterVolume { get; init; }

    [JsonPropertyName("avg")] public string Avg { get; init; }

    [JsonPropertyName("high")] public string High { get; init; }

    [JsonPropertyName("low")] public string Low { get; init; }

    [JsonPropertyName("open")] public string Open { get; init; }

    [JsonPropertyName("close")] public string Close { get; init; }
}