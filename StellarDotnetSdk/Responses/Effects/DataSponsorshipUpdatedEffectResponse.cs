using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents data_sponsorship_created effect response.
/// </summary>
public class DataSponsorshipUpdatedEffectResponse : EffectResponse
{
    public override int TypeId => 67;

    [JsonPropertyName("new_sponsor")]
    public string NewSponsor { get; init; }

    [JsonPropertyName("former_sponsor")]
    public string FormerSponsor { get; init; }

    [JsonPropertyName("data_name")]
    public string DataName { get; init; }
}