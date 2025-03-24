using Newtonsoft.Json;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class OrderBookResponse
{
    [JsonProperty(PropertyName = "base")]
    [JsonConverter(typeof(AssetJsonConverter))]
    public Asset OrderBookBase { get; init; }

    [JsonProperty(PropertyName = "counter")]
    [JsonConverter(typeof(AssetJsonConverter))]
    public Asset Counter { get; init; }

    [JsonProperty(PropertyName = "asks")] public Row[] Asks { get; init; }

    [JsonProperty(PropertyName = "bids")] public Row[] Bids { get; init; }

    /// Represents order book row.
    public class Row
    {
        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; init; }

        /// <summary>
        ///     The ask/bid price.
        /// </summary>
        [JsonProperty(PropertyName = "price")]
        public string Price { get; init; }

        /// <summary>
        ///     The ask/bid price as a ratio.
        /// </summary>
        [JsonProperty(PropertyName = "price_r")]
        public StellarDotnetSdk.Price PriceR { get; init; }
    }
}