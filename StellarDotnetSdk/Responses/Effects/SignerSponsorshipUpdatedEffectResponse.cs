using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents signer_sponsorship_updated effect response.
/// </summary>
public class SignerSponsorshipUpdatedEffectResponse : EffectResponse
{
    public override int TypeId => 73;

    [JsonPropertyName("signer")]
    public string Signer { get; init; }

    [JsonPropertyName("former_sponsor")]
    public string FormerSponsor { get; init; }

    [JsonPropertyName("new_sponsor")]
    public string NewSponsor { get; init; }
}