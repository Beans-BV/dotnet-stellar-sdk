using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Instructions for completing an off-chain deposit.
///     Provides specific details about how to complete a deposit, typically
///     containing account numbers, routing codes, or other payment identifiers
///     needed to send funds to the anchor.
/// </summary>
public sealed record DepositInstruction
{
    /// <summary>
    ///     The value of the field.
    /// </summary>
    [JsonPropertyName("value")]
    public required string Value { get; init; }

    /// <summary>
    ///     A human-readable description of the field. This can be used by an anchor
    ///     to provide any additional information about fields that are not defined
    ///     in the SEP-9 standard.
    /// </summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }
}