using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses;

public class TradeAggregationResponse : Response
{
    public TradeAggregationResponse(string timestamp, string tradeCount, string baseVolume, string counterVolume,
        string avg, string high, string low, string open, string close)
    {
        Timestamp = timestamp;
        TradeCount = tradeCount;
        BaseVolume = baseVolume;
        CounterVolume = counterVolume;
        Avg = avg;
        High = high;
        Low = low;
        Open = open;
        Close = close;
    }

    [JsonProperty(PropertyName = "timestamp")]
    public string Timestamp { get; }

    [JsonProperty(PropertyName = "trade_count")]
    public string TradeCount { get; }

    [JsonProperty(PropertyName = "base_volume")]
    public string BaseVolume { get; }

    [JsonProperty(PropertyName = "counter_volume")]
    public string CounterVolume { get; }

    [JsonProperty(PropertyName = "avg")] public string Avg { get; }

    [JsonProperty(PropertyName = "high")] public string High { get; }

    [JsonProperty(PropertyName = "low")] public string Low { get; }

    [JsonProperty(PropertyName = "open")] public string Open { get; }

    [JsonProperty(PropertyName = "close")] public string Close { get; }
}