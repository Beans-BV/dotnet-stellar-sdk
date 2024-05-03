using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents trustline_sponsorship_removed effect response.
/// </summary>
public class TrustlineSponsorshipRemovedEffectResponse : EffectResponse
{
    public override int TypeId => 65;

    [JsonProperty(PropertyName = "asset")] public string Asset { get; init; }

    [JsonProperty(PropertyName = "former_sponsor")]
    public string FormerSponsor { get; init; }
}