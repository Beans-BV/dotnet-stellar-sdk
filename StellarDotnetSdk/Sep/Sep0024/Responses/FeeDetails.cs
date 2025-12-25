using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0024.Responses;

/// <summary>
///     Description of fee charged by the anchor.
///     Contains the total fee amount, the asset in which fees are calculated, and an optional breakdown
///     detailing the individual fee components. This replaces the deprecated amount_fee field.
///     If quote_id is present, it should match the referenced quote's fee object.
/// </summary>
public class FeeDetails : Response
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FeeDetails" /> class.
    /// </summary>
    /// <param name="total">The total amount of fee applied.</param>
    /// <param name="asset">The asset in which the fee is applied, represented through the Asset Identification Format.</param>
    /// <param name="breakdown">Optional array of objects detailing the fees that were used to calculate the conversion price.</param>
    [JsonConstructor]
    public FeeDetails(decimal total, string asset, List<FeeBreakdown>? breakdown = null)
    {
        Total = total;
        Asset = asset;
        Breakdown = breakdown;
    }

    /// <summary>
    ///     Gets the total amount of fee applied.
    /// </summary>
    [JsonPropertyName("total")]
    public decimal Total { get; }

    /// <summary>
    ///     Gets the asset in which the fee is applied, represented through the Asset Identification Format.
    /// </summary>
    [JsonPropertyName("asset")]
    public string Asset { get; }

    /// <summary>
    ///     Gets an optional array of objects detailing the fees that were used to calculate the conversion price.
    ///     This can be used to detail the price components for the end-user.
    ///     If breakdown is provided, sum(breakdown.amount) should equal total.
    /// </summary>
    [JsonPropertyName("breakdown")]
    public List<FeeBreakdown>? Breakdown { get; }
}

