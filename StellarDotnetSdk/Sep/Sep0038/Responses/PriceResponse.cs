using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Sep.Sep0038.Responses;

/// <summary>
///     Response of SEP-38 <c>GET /price</c>: the indicative price for one asset pair.
/// </summary>
/// <remarks>
///     <para>
///         The price is indicative; the actual price is calculated at conversion time. Use <c>POST /quote</c> for a
///         firm price.
///     </para>
///     <para>
///         Every amount and price is read exactly: a value that <see cref="decimal" /> cannot hold without rounding
///         fails deserialization instead of being approximated.
///     </para>
/// </remarks>
public sealed record PriceResponse
{
    /// <summary>
    ///     The total conversion price for one unit of the buy asset in terms of the sell asset, including fees.
    ///     <c>sell_amount = total_price * buy_amount</c>.
    /// </summary>
    [JsonPropertyName("total_price")]
    [JsonRequired]
    [JsonConverter(typeof(ExactDecimalJsonConverter))]
    public required decimal TotalPrice { get; init; }

    /// <summary>
    ///     The conversion price for one unit of the buy asset in terms of the sell asset, excluding fees.
    /// </summary>
    [JsonPropertyName("price")]
    [JsonRequired]
    [JsonConverter(typeof(ExactDecimalJsonConverter))]
    public required decimal Price { get; init; }

    /// <summary>
    ///     The amount of the sell asset the anchor will exchange. It can differ from the requested
    ///     <c>sell_amount</c>, depending on how the anchor applies fees.
    /// </summary>
    [JsonPropertyName("sell_amount")]
    [JsonRequired]
    [JsonConverter(typeof(ExactDecimalJsonConverter))]
    public required decimal SellAmount { get; init; }

    /// <summary>
    ///     The amount of the buy asset the anchor will provide. It can differ from the requested
    ///     <c>buy_amount</c>, depending on how the anchor applies fees.
    /// </summary>
    [JsonPropertyName("buy_amount")]
    [JsonRequired]
    [JsonConverter(typeof(ExactDecimalJsonConverter))]
    public required decimal BuyAmount { get; init; }

    /// <summary>
    ///     The fee used to calculate the conversion price.
    /// </summary>
    [JsonPropertyName("fee")]
    [JsonRequired]
    public required QuoteFee Fee { get; init; }
}
