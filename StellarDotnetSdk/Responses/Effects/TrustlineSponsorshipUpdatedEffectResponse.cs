using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents trustline_sponsorship_updated effect response.
/// </summary>
public class TrustlineSponsorshipUpdatedEffectResponse : EffectResponse
{
    public override int TypeId => 64;

    [JsonProperty(PropertyName = "asset")] public string Asset { get; init; }

    [JsonProperty(PropertyName = "former_sponsor")]
    public string FormerSponsor { get; init; }

    [JsonProperty(PropertyName = "new_sponsor")]
    public string NewSponsor { get; init; }
}