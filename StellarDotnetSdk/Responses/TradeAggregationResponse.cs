using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents aggregated trade data for a specific time period.
///     Trade aggregations provide OHLC (Open, High, Low, Close) candlestick data
///     for trading pairs on the Stellar DEX.
/// </summary>
public sealed class TradeAggregationResponse : Response
{
    /// <summary>
    ///     The start time for this trade aggregation. Represented as milliseconds since epoch.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public required string Timestamp { get; init; }

    /// <summary>
    ///     The total number of trades aggregated.
    /// </summary>
    [JsonPropertyName("trade_count")]
    public required string TradeCount { get; init; }

    /// <summary>
    ///     The total volume of base asset.
    /// </summary>
    [JsonPropertyName("base_volume")]
    public required string BaseVolume { get; init; }

    /// <summary>
    ///     The total volume of counter asset.
    /// </summary>
    [JsonPropertyName("counter_volume")]
    public required string CounterVolume { get; init; }

    /// <summary>
    ///     The weighted average price of counter asset in terms of base asset.
    /// </summary>
    [JsonPropertyName("avg")]
    public required string Avg { get; init; }

    /// <summary>
    ///     The highest price for this time period.
    /// </summary>
    [JsonPropertyName("high")]
    public required string High { get; init; }

    /// <summary>
    ///     The highest price for this time period as a rational number.
    /// </summary>
    [JsonPropertyName("high_r")]
    public required Price HighR { get; init; }

    /// <summary>
    ///     The lowest price for this time period.
    /// </summary>
    [JsonPropertyName("low")]
    public required string Low { get; init; }

    /// <summary>
    ///     The lowest price for this time period as a rational number.
    /// </summary>
    [JsonPropertyName("low_r")]
    public required Price LowR { get; init; }

    /// <summary>
    ///     The price as seen on first trade aggregated.
    /// </summary>
    [JsonPropertyName("open")]
    public required string Open { get; init; }

    /// <summary>
    ///     The price as seen on first trade aggregated as a rational number.
    /// </summary>
    [JsonPropertyName("open_r")]
    public required Price OpenR { get; init; }

    /// <summary>
    ///     The price as seen on last trade aggregated.
    /// </summary>
    [JsonPropertyName("close")]
    public required string Close { get; init; }

    /// <summary>
    ///     The price as seen on last trade aggregated as a rational number.
    /// </summary>
    [JsonPropertyName("close_r")]
    public required Price CloseR { get; init; }
}