using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents data_sponsorship_created effect response.
/// </summary>
public class DataSponsorshipRemovedEffectResponse : EffectResponse
{
    public override int TypeId => 68;

    [JsonProperty(PropertyName = "former_sponsor")]
    public string FormerSponsor { get; init; }

    [JsonProperty(PropertyName = "data_name")]
    public string DataName { get; init; }
}