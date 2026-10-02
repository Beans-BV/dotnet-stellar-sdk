using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0038.Responses;

/// <summary>
///     A method a client can use to deliver funds to the anchor (an entry of <c>sell_delivery_methods</c>) or to
///     receive funds from it (an entry of <c>buy_delivery_methods</c>), as listed by <c>GET /info</c>.
/// </summary>
/// <remarks>
///     Pass <see cref="Name" /> as <c>SellDeliveryMethod</c> or <c>BuyDeliveryMethod</c> in a price or quote
///     request. The methods should match the <c>funding_methods</c> of the SEP (for example SEP-6) the quote will
///     be used with.
/// </remarks>
public sealed record DeliveryMethod
{
    /// <summary>
    ///     The value to use in <c>sell_delivery_method</c> / <c>buy_delivery_method</c> request parameters, for
    ///     example <c>PIX</c>, <c>ACH</c> or <c>cash</c>.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonRequired]
    public required string Name { get; init; }

    /// <summary>
    ///     A human-readable description of the method identified by <see cref="Name" />.
    /// </summary>
    [JsonPropertyName("description")]
    [JsonRequired]
    public required string Description { get; init; }
}
