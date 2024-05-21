using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents data_sponsorship_created effect response.
/// </summary>
public class DataSponsorshipCreatedEffectResponse : EffectResponse
{
    public override int TypeId => 66;

    [JsonPropertyName("sponsor")]
    public string Sponsor { get; init; }

    [JsonPropertyName("data_name")]
    public string DataName { get; init; }
}