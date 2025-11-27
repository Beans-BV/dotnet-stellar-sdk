using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents an order book for a trading pair on the Stellar decentralized exchange.
///     Contains both buy orders (bids) and sell orders (asks).
/// </summary>
public sealed class OrderBookResponse : Response
{
    /// <summary>
    ///     The base asset for this order book (the asset being sold in asks, bought in bids).
    /// </summary>
    [JsonPropertyName("base")]
    [JsonConverter(typeof(AssetJsonConverter))]
    public required Asset OrderBookBase { get; init; }

    /// <summary>
    ///     The counter asset for this order book (the asset being bought in asks, sold in bids).
    /// </summary>
    [JsonPropertyName("counter")]
    [JsonConverter(typeof(AssetJsonConverter))]
    public required Asset Counter { get; init; }

    /// <summary>
    ///     The prices and amounts for the sellside of the asset pair.
    /// </summary>
    [JsonPropertyName("asks")]
    public required Row[] Asks { get; init; }

    /// <summary>
    ///     The prices and amounts for the buyside of the asset pair.
    /// </summary>
    [JsonPropertyName("bids")]
    public required Row[] Bids { get; init; }

    /// <summary>
    ///     Represents a single row in the order book (either an ask or a bid).
    /// </summary>
    public sealed class Row
    {
        /// <summary>
        ///     The amount of the base asset available at this price level.
        ///     Represented as a string to preserve precision.
        /// </summary>
        [JsonPropertyName("amount")]
        public required string Amount { get; init; }

        /// <summary>
        ///     The ask/bid price as a decimal string.
        ///     Represents how much of the counter asset is needed for 1 unit of the base asset.
        /// </summary>
        [JsonPropertyName("price")]
        public required string Price { get; init; }

        /// <summary>
        ///     The ask/bid price as a rational number (numerator/denominator).
        /// </summary>
        [JsonPropertyName("price_r")]
        public required Price PriceR { get; init; }
    }
}