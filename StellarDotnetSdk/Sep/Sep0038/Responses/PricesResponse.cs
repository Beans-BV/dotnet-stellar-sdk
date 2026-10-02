using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Sep.Sep0038.Responses;

/// <summary>
///     Response of SEP-38 <c>GET /prices</c>: indicative prices of the assets available in exchange for the asset
///     given in the request.
/// </summary>
/// <remarks>
///     Which list is populated depends on the request: a request with <c>sell_asset</c> is answered with
///     <see cref="BuyAssets" />, one with <c>buy_asset</c> with <see cref="SellAssets" />;
///     <see cref="QuoteService.PricesAsync" /> rejects a response that lacks the list the request calls for. These
///     prices are indicative; the actual price is calculated at conversion time.
/// </remarks>
public sealed record PricesResponse
{
    /// <summary>
    ///     The assets the client can receive for the requested <c>sell_asset</c>, with their prices. Present when
    ///     the request specified <c>sell_asset</c>.
    /// </summary>
    [JsonPropertyName("buy_assets")]
    [JsonConverter(typeof(NonNullElementListJsonConverter<AssetPrice>))]
    public IReadOnlyList<AssetPrice>? BuyAssets { get; init; }

    /// <summary>
    ///     The assets the client can pay with to receive the requested <c>buy_asset</c>, with their prices. Present
    ///     when the request specified <c>buy_asset</c>.
    /// </summary>
    [JsonPropertyName("sell_assets")]
    [JsonConverter(typeof(NonNullElementListJsonConverter<AssetPrice>))]
    public IReadOnlyList<AssetPrice>? SellAssets { get; init; }
}
