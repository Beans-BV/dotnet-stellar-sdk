using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Sep.Sep0038.Responses;

/// <summary>
///     An asset the anchor offers in exchange for one or more of the other assets listed by <c>GET /info</c>.
/// </summary>
/// <remarks>
///     The anchor may not support a trading pair between every Stellar asset and off-chain asset it lists; use
///     <c>GET /prices</c> to see which pairs are supported.
/// </remarks>
public sealed record AssetInfo
{
    /// <summary>
    ///     The asset in the SEP-38 Asset Identification Format, for example
    ///     <c>stellar:USDC:GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN</c> or <c>iso4217:BRL</c>. Use
    ///     <see cref="AssetIdentifier.Parse" /> to split it into its parts.
    /// </summary>
    [JsonPropertyName("asset")]
    [JsonRequired]
    public required string Asset { get; init; }

    /// <summary>
    ///     Only for non-Stellar assets: the methods a client can use to sell (deliver) this asset to the anchor.
    ///     <see langword="null" /> when the anchor does not need a delivery method to quote accurately.
    /// </summary>
    [JsonPropertyName("sell_delivery_methods")]
    [JsonConverter(typeof(NonNullElementListJsonConverter<DeliveryMethod>))]
    public IReadOnlyList<DeliveryMethod>? SellDeliveryMethods { get; init; }

    /// <summary>
    ///     Only for non-Stellar assets: the methods a client can use to buy (receive) this asset from the anchor.
    ///     <see langword="null" /> when the anchor does not need a delivery method to quote accurately.
    /// </summary>
    [JsonPropertyName("buy_delivery_methods")]
    [JsonConverter(typeof(NonNullElementListJsonConverter<DeliveryMethod>))]
    public IReadOnlyList<DeliveryMethod>? BuyDeliveryMethods { get; init; }

    /// <summary>
    ///     Only for fiat assets: the ISO 3166-2 (or ISO 3166-1 alpha-2) codes of the countries where the anchor
    ///     operates for this asset. When more than one is listed, pass the user's as <c>CountryCode</c> in price
    ///     and quote requests.
    /// </summary>
    [JsonPropertyName("country_codes")]
    [JsonConverter(typeof(NonNullElementListJsonConverter<string>))]
    public IReadOnlyList<string>? CountryCodes { get; init; }
}
