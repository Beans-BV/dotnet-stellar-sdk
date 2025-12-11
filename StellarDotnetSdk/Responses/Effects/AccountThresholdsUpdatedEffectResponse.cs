using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the account_thresholds_updated effect response.
///     This effect occurs when an account's signing thresholds are changed.
/// </summary>
public sealed class AccountThresholdsUpdatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 4;

    /// <summary>
    ///     The new low threshold value. Operations with this security level
    ///     require signatures with weight equal to or greater than this value.
    /// </summary>
    [JsonPropertyName("low_threshold")]
    public required int LowThreshold { get; init; }

    /// <summary>
    ///     The new medium threshold value. Operations with this security level
    ///     require signatures with weight equal to or greater than this value.
    /// </summary>
    [JsonPropertyName("med_threshold")]
    public required int MedThreshold { get; init; }

    /// <summary>
    ///     The new high threshold value. Operations with this security level
    ///     require signatures with weight equal to or greater than this value.
    /// </summary>
    [JsonPropertyName("high_threshold")]
    public required int HighThreshold { get; init; }
}