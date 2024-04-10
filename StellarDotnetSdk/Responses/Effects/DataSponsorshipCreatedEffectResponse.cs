using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents data_sponsorship_created effect response.
/// </summary>
public class DataSponsorshipCreatedEffectResponse : EffectResponse
{
    public override int TypeId => 66;

    [JsonProperty(PropertyName = "sponsor")]
    public string Sponsor { get; init; }

    [JsonProperty(PropertyName = "data_name")]
    public string DataName { get; init; }
}