using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Individual fee component within a transaction's fee breakdown.
///     Represents a single named fee that contributes to the total transaction fee.
/// </summary>
public sealed class FeeDetailsDetails
{
    /// <summary>
    ///     The name of the fee, for example ACH fee, Brazilian conciliation fee,
    ///     Service fee, etc.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    ///     The amount of asset applied. If fee_details.details is provided,
    ///     sum(fee_details.details.amount) should be equals fee_details.total.
    /// </summary>
    [JsonPropertyName("amount")]
    public required decimal Amount { get; init; }

    /// <summary>
    ///     A text describing the fee.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }
}

