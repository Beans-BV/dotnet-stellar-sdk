using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Sep.Sep0038.Responses;

/// <summary>
///     A firm SEP-38 quote, as returned by <c>POST /quote</c> and <c>GET /quote/:id</c>.
/// </summary>
/// <remarks>
///     <para>
///         Pass <see cref="Id" /> as the <c>quote_id</c> of a SEP-6, SEP-24 or SEP-31 transaction to use the quote.
///         The anchor holds the quoted amount in reserve until <see cref="ExpiresAt" />.
///     </para>
///     <para>
///         Every amount and price is read exactly: a value <see cref="decimal" /> cannot hold without rounding fails
///         deserialization instead of being approximated.
///     </para>
/// </remarks>
public sealed record QuoteResponse
{
    /// <summary>
    ///     The unique identifier of the quote, to be used in other SEPs.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonRequired]
    public required string Id { get; init; }

    /// <summary>
    ///     The time by which the anchor must receive funds from the client. The specification defines it as UTC;
    ///     a value sent without an offset is read as UTC, not as local time.
    /// </summary>
    [JsonPropertyName("expires_at")]
    [JsonRequired]
    [JsonConverter(typeof(UtcDateTimeOffsetJsonConverter))]
    public required DateTimeOffset ExpiresAt { get; init; }

    /// <summary>
    ///     The total conversion price for one unit of <see cref="BuyAsset" /> in terms of <see cref="SellAsset" />,
    ///     including fees. <c>sell_amount = total_price * buy_amount</c>.
    /// </summary>
    /// <remarks>
    ///     Always set on a quote from <see cref="QuoteService.PostQuoteAsync" />, which rejects a response without
    ///     it. <see langword="null" /> only when <c>GET /quote/:id</c> omits it: that endpoint's response table in the
    ///     specification does not list <c>total_price</c>.
    /// </remarks>
    [JsonPropertyName("total_price")]
    [JsonConverter(typeof(ExactDecimalJsonConverter))]
    public decimal? TotalPrice { get; init; }

    /// <summary>
    ///     The conversion price for one unit of <see cref="BuyAsset" /> in terms of <see cref="SellAsset" />,
    ///     excluding fees.
    /// </summary>
    [JsonPropertyName("price")]
    [JsonRequired]
    [JsonConverter(typeof(ExactDecimalJsonConverter))]
    public required decimal Price { get; init; }

    /// <summary>
    ///     The asset the client sells, in the SEP-38 Asset Identification Format.
    /// </summary>
    [JsonPropertyName("sell_asset")]
    [JsonRequired]
    public required string SellAsset { get; init; }

    /// <summary>
    ///     The amount of <see cref="SellAsset" /> to be exchanged. It can differ from the requested amount,
    ///     depending on how the anchor applies fees.
    /// </summary>
    [JsonPropertyName("sell_amount")]
    [JsonRequired]
    [JsonConverter(typeof(ExactDecimalJsonConverter))]
    public required decimal SellAmount { get; init; }

    /// <summary>
    ///     The method by which the user plans to deliver an off-chain asset to the anchor. Present only if it was
    ///     specified when the quote was requested.
    /// </summary>
    [JsonPropertyName("sell_delivery_method")]
    public string? SellDeliveryMethod { get; init; }

    /// <summary>
    ///     The asset the client buys, in the SEP-38 Asset Identification Format.
    /// </summary>
    [JsonPropertyName("buy_asset")]
    [JsonRequired]
    public required string BuyAsset { get; init; }

    /// <summary>
    ///     The amount of <see cref="BuyAsset" /> to be exchanged. It can differ from the requested amount,
    ///     depending on how the anchor applies fees.
    /// </summary>
    [JsonPropertyName("buy_amount")]
    [JsonRequired]
    [JsonConverter(typeof(ExactDecimalJsonConverter))]
    public required decimal BuyAmount { get; init; }

    /// <summary>
    ///     The method by which the user plans to receive an off-chain asset from the anchor. Present only if it
    ///     was specified when the quote was requested.
    /// </summary>
    [JsonPropertyName("buy_delivery_method")]
    public string? BuyDeliveryMethod { get; init; }

    /// <summary>
    ///     The fee used to calculate the conversion price.
    /// </summary>
    [JsonPropertyName("fee")]
    [JsonRequired]
    public required QuoteFee Fee { get; init; }
}
