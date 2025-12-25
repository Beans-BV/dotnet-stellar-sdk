using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Describes a field that needs to be provided for a transaction.
///     Anchors use this to specify additional fields required for deposits or
///     withdrawals beyond the standard parameters. Each field includes a
///     description and whether it's optional.
/// </summary>
public sealed record AnchorField
{
    /// <summary>
    ///     Description of field to show to user.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    ///     If field is optional. Defaults to false.
    /// </summary>
    [JsonPropertyName("optional")]
    public bool? Optional { get; init; }

    /// <summary>
    ///     List of possible values for the field.
    /// </summary>
    [JsonPropertyName("choices")]
    public IReadOnlyList<string>? Choices { get; init; }
}