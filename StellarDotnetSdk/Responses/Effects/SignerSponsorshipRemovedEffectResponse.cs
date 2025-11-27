using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents signer_sponsorship_removed effect response.
/// </summary>
public class SignerSponsorshipRemovedEffectResponse : EffectResponse
{
    public override int TypeId => 74;

    [JsonPropertyName("signer")]
    public string Signer { get; init; }

    [JsonPropertyName("former_sponsor")]
    public string FormerSponsor { get; init; }
}