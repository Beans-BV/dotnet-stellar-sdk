using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents trustline_sponsorship_removed effect response.
/// </summary>
public class TrustlineSponsorshipRemovedEffectResponse : EffectResponse
{
    public override int TypeId => 65;

    [JsonPropertyName("asset")]
    public string Asset { get; init; }

    [JsonPropertyName("former_sponsor")]
    public string FormerSponsor { get; init; }
}