using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class OrderBookResponse : Response
{
    [JsonPropertyName("base")]
    [JsonConverter(typeof(AssetJsonConverter))]
    public Asset OrderBookBase { get; init; }

    [JsonPropertyName("counter")]
    [JsonConverter(typeof(AssetJsonConverter))]
    public Asset Counter { get; init; }

    [JsonPropertyName("asks")]
    public Row[] Asks { get; init; }

    [JsonPropertyName("bids")]
    public Row[] Bids { get; init; }

    /// Represents order book row.
    public class Row
    {
        [JsonPropertyName("amount")]
        public string Amount { get; init; }

        /// <summary>
        ///     The ask/bid price.
        /// </summary>
        [JsonPropertyName("price")]
        public string Price { get; init; }

        /// <summary>
        ///     The ask/bid price as a ratio.
        /// </summary>
        [JsonPropertyName("price_r")]
        public Price PriceRatio { get; init; }
    }
}