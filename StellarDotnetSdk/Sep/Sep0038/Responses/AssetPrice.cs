using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Sep.Sep0038.Responses;

/// <summary>
///     An indicative price for one asset, as an entry of the <c>buy_assets</c> or <c>sell_assets</c> array of
///     <c>GET /prices</c>.
/// </summary>
public sealed record AssetPrice
{
    /// <summary>
    ///     The asset in the SEP-38 Asset Identification Format.
    /// </summary>
    [JsonPropertyName("asset")]
    [JsonRequired]
    public required string Asset { get; init; }

    /// <summary>
    ///     The indicative price, always quoted as the amount of the asset sold for one unit of the asset bought, so
    ///     its direction depends on which side the request gave.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         In <see cref="PricesResponse.BuyAssets" /> (a request with <c>sell_asset</c>), it is the price of one
    ///         unit of <see cref="Asset" /> in terms of the requested sell asset: selling BRL for USDC at
    ///         <c>5.42</c> means one USDC costs 5.42 BRL, as in the specification's sell-side example.
    ///     </para>
    ///     <para>
    ///         In <see cref="PricesResponse.SellAssets" /> (a request with <c>buy_asset</c>), it is the price of one
    ///         unit of the requested buy asset in terms of <see cref="Asset" />: buying USDC with BRL at <c>5.42</c>
    ///         means one USDC costs 5.42 BRL, as in the specification's buy-side example.
    ///     </para>
    ///     <para>
    ///         Read exactly: a value <see cref="decimal" /> cannot hold without rounding fails deserialization instead
    ///         of being approximated.
    ///     </para>
    /// </remarks>
    [JsonPropertyName("price")]
    [JsonRequired]
    [JsonConverter(typeof(ExactDecimalJsonConverter))]
    public required decimal Price { get; init; }

    /// <summary>
    ///     The number of decimals needed to represent <see cref="Asset" />.
    /// </summary>
    [JsonPropertyName("decimals")]
    [JsonRequired]
    public required int Decimals { get; init; }
}
