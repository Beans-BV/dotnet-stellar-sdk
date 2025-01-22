using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents trustline_sponsorship_updated effect response.
/// </summary>
public class TrustlineSponsorshipUpdatedEffectResponse : EffectResponse
{
    public override int TypeId => 64;

    [JsonPropertyName("asset")] public string Asset { get; init; }

    [JsonPropertyName("former_sponsor")]
    public string FormerSponsor { get; init; }

    [JsonPropertyName("new_sponsor")]
    public string NewSponsor { get; init; }
}