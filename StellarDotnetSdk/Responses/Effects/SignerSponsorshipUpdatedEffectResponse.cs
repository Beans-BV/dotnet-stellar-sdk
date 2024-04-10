using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents signer_sponsorship_updated effect response.
/// </summary>
public class SignerSponsorshipUpdatedEffectResponse : EffectResponse
{
    public override int TypeId => 73;

    [JsonProperty(PropertyName = "signer")]
    public string Signer { get; init; }

    [JsonProperty(PropertyName = "former_sponsor")]
    public string FormerSponsor { get; init; }

    [JsonProperty(PropertyName = "new_sponsor")]
    public string NewSponsor { get; init; }
}