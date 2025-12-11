using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the data_removed effect response.
///     This effect occurs when a data entry is removed from an account.
/// </summary>
public sealed class DataRemovedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 41;

    /// <summary>
    ///     Name of the removed data entry.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}