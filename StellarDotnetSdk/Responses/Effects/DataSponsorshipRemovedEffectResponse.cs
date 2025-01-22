using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents data_sponsorship_created effect response.
/// </summary>
public class DataSponsorshipRemovedEffectResponse : EffectResponse
{
    public override int TypeId => 68;

    [JsonPropertyName("former_sponsor")]
    public string FormerSponsor { get; init; }

    [JsonPropertyName("data_name")]
    public string DataName { get; init; }
}