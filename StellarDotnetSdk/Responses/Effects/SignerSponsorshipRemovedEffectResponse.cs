using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents signer_sponsorship_removed effect response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/effect.html
///     <seealso cref="Requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class SignerSponsorshipRemovedEffectResponse : EffectResponse
{
    public SignerSponsorshipRemovedEffectResponse()
    {
    }

    public SignerSponsorshipRemovedEffectResponse(string signer, string formerSponsor)
    {
        Signer = signer;
        FormerSponsor = formerSponsor;
    }

    public override int TypeId => 74;

    [JsonProperty(PropertyName = "signer")]
    public string Signer { get; private set; }

    [JsonProperty(PropertyName = "former_sponsor")]
    public string FormerSponsor { get; private set; }
}