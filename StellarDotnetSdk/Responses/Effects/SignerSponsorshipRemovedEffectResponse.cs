using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents signer_sponsorship_removed effect response.
/// </summary>
public class SignerSponsorshipRemovedEffectResponse : EffectResponse
{
    public override int TypeId => 74;

    [JsonProperty(PropertyName = "signer")]
    public string Signer { get; init; }

    [JsonProperty(PropertyName = "former_sponsor")]
    public string FormerSponsor { get; init; }
}