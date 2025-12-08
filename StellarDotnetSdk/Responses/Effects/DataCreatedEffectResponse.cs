using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the data_created effect response.
///     This effect occurs when a data entry is created on an account.
/// </summary>
public sealed class DataCreatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 40;

    /// <summary>
    ///     Name of the created data entry.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    ///     Value of the created data entry.
    /// </summary>
    [JsonPropertyName("value")]
    public required string Value { get; init; }
}