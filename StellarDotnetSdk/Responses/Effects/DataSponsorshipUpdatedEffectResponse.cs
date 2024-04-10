using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents data_sponsorship_created effect response.
/// </summary>
public class DataSponsorshipUpdatedEffectResponse : EffectResponse
{
    public override int TypeId => 67;

    [JsonProperty(PropertyName = "new_sponsor")]
    public string NewSponsor { get; init; }

    [JsonProperty(PropertyName = "former_sponsor")]
    public string FormerSponsor { get; init; }

    [JsonProperty(PropertyName = "data_name")]
    public string DataName { get; init; }
}