using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the data_updated effect response.
///     This effect occurs when a data entry is updated on an account.
/// </summary>
public sealed class DataUpdatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 42;

    /// <summary>
    ///     Name of the updated data entry.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    ///     Value of the updated data entry.
    /// </summary>
    [JsonPropertyName("value")]
    public required string Value { get; init; }
}