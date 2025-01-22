using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents trustline_sponsorship_created effect response.
/// </summary>
public class TrustlineSponsorshipCreatedEffectResponse : EffectResponse
{
    public override int TypeId => 63;

    [JsonPropertyName("asset")] public string Asset { get; init; }

    [JsonPropertyName("sponsor")]
    public string Sponsor { get; init; }
}