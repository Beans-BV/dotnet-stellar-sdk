using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents account thresholds.
/// </summary>
public class Thresholds
{
    [JsonProperty(PropertyName = "low_threshold")]
    public int LowThreshold { get; init; }

    [JsonProperty(PropertyName = "med_threshold")]
    public int MedThreshold { get; init; }

    [JsonProperty(PropertyName = "high_threshold")]
    public int HighThreshold { get; init; }
}