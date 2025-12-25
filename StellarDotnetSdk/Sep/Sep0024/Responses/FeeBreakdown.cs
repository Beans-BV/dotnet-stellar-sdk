using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0024.Responses;

/// <summary>
///     Details of an individual fee component used to calculate the total fee.
///     Multiple breakdown items may exist for a single transaction, each representing a different fee component
///     (e.g., ACH fee, service fee, etc.).
/// </summary>
public class FeeBreakdown : Response
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FeeBreakdown" /> class.
    /// </summary>
    /// <param name="name">The name of the fee, for example 'ACH fee', 'Brazilian conciliation fee', 'Service fee', etc.</param>
    /// <param name="amount">The amount of asset applied. If fee_details.breakdown is provided, sum(fee_details.breakdown.amount) should equal fee_details.total.</param>
    /// <param name="description">Optional text describing the fee.</param>
    [JsonConstructor]
    public FeeBreakdown(string name, decimal amount, string? description = null)
    {
        Name = name;
        Amount = amount;
        Description = description;
    }

    /// <summary>
    ///     Gets the name of the fee, for example 'ACH fee', 'Brazilian conciliation fee', 'Service fee', etc.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; }

    /// <summary>
    ///     Gets optional text describing the fee.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; }

    /// <summary>
    ///     Gets the amount of asset applied. If fee_details.breakdown is provided,
    ///     sum(fee_details.breakdown.amount) should equal fee_details.total.
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; }
}

