using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents account_thresholds_updated effect response.
/// </summary>
public class AccountThresholdsUpdatedEffectResponse : EffectResponse
{
    public override int TypeId => 4;

    [JsonProperty(PropertyName = "low_threshold")]
    public int LowThreshold { get; init; }

    [JsonProperty(PropertyName = "med_threshold")]
    public int MedThreshold { get; init; }

    [JsonProperty(PropertyName = "high_threshold")]
    public int HighThreshold { get; init; }
}