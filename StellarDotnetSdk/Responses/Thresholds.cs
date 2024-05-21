using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents account thresholds.
/// </summary>
public class Thresholds
{
    [JsonPropertyName("low_threshold")]
    public int LowThreshold { get; init; }

    [JsonPropertyName("med_threshold")]
    public int MedThreshold { get; init; }

    [JsonPropertyName("high_threshold")]
    public int HighThreshold { get; init; }
}