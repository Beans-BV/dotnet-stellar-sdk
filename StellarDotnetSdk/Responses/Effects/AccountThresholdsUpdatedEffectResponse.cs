using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents account_thresholds_updated effect response.
/// </summary>
public class AccountThresholdsUpdatedEffectResponse : EffectResponse
{
    public override int TypeId => 4;

    [JsonPropertyName("low_threshold")]
    public int LowThreshold { get; init; }

    [JsonPropertyName("med_threshold")]
    public int MedThreshold { get; init; }

    [JsonPropertyName("high_threshold")]
    public int HighThreshold { get; init; }
}