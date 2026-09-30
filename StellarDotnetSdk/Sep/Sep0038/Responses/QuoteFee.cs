using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Sep.Sep0038.Responses;

/// <summary>
///     The fee used to calculate a SEP-38 conversion price (the <c>fee</c> object of <c>GET /price</c>,
///     <c>POST /quote</c> and <c>GET /quote/:id</c>). It can be used to show the price components to the user.
/// </summary>
/// <remarks>
///     With <c>fee</c> in the sell asset, <c>sell_amount - fee = price * buy_amount</c>; with it in the buy asset,
///     <c>sell_amount = price * (buy_amount + fee)</c>. In both cases <c>sell_amount = total_price * buy_amount</c>.
/// </remarks>
public sealed record QuoteFee
{
    /// <summary>
    ///     The total amount of fee applied, in <see cref="Asset" />. Read exactly, without rounding.
    /// </summary>
    [JsonPropertyName("total")]
    [JsonRequired]
    [JsonConverter(typeof(ExactDecimalJsonConverter))]
    public required decimal Total { get; init; }

    /// <summary>
    ///     The asset in which the fee is applied, in the SEP-38 Asset Identification Format.
    /// </summary>
    [JsonPropertyName("asset")]
    [JsonRequired]
    public required string Asset { get; init; }

    /// <summary>
    ///     An optional breakdown of the fee. When present, the sum of the <see cref="QuoteFeeDetail.Amount" />
    ///     values should equal <see cref="Total" />.
    /// </summary>
    [JsonPropertyName("details")]
    [JsonConverter(typeof(NonNullElementListJsonConverter<QuoteFeeDetail>))]
    public IReadOnlyList<QuoteFeeDetail>? Details { get; init; }
}
