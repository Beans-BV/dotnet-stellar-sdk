using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents trustline_sponsorship_created effect response.
/// </summary>
public class TrustlineSponsorshipCreatedEffectResponse : EffectResponse
{
    public override int TypeId => 63;

    [JsonProperty(PropertyName = "asset")] public string Asset { get; init; }

    [JsonProperty(PropertyName = "sponsor")]
    public string Sponsor { get; init; }
}