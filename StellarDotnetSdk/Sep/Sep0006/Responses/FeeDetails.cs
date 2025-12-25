using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Detailed breakdown of fees applied to a transaction.
///     Provides comprehensive fee information including the total fee amount, the asset
///     in which the fee is charged, and optionally a detailed breakdown of individual
///     fee components that make up the total.
/// </summary>
public sealed class FeeDetails
{
    /// <summary>
    ///     The total amount of fee applied.
    /// </summary>
    [JsonPropertyName("total")]
    public required decimal Total { get; init; }

    /// <summary>
    ///     The asset in which the fee is applied, represented through the
    ///     Asset Identification Format.
    /// </summary>
    [JsonPropertyName("asset")]
    public required string Asset { get; init; }

    /// <summary>
    ///     An array of objects detailing the fees that were used to
    ///     calculate the conversion price. This can be used to detail the price
    ///     components for the end-user.
    /// </summary>
    [JsonPropertyName("details")]
    public IReadOnlyList<FeeDetailsDetails>? Details { get; init; }
}