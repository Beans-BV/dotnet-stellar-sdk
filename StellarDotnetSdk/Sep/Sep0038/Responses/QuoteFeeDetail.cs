using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Sep.Sep0038.Responses;

/// <summary>
///     One component of a <see cref="QuoteFee" />, such as a service fee or a payment-rail fee.
/// </summary>
public sealed record QuoteFeeDetail
{
    /// <summary>
    ///     The name of the fee, for example <c>ACH fee</c>, <c>Brazilian conciliation fee</c> or <c>Service fee</c>.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonRequired]
    public required string Name { get; init; }

    /// <summary>
    ///     An optional text describing the fee.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    ///     The amount of this fee, in <see cref="QuoteFee.Asset" />. Read exactly, without rounding.
    /// </summary>
    [JsonPropertyName("amount")]
    [JsonRequired]
    [JsonConverter(typeof(ExactDecimalJsonConverter))]
    public required decimal Amount { get; init; }
}
