using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents signer_sponsorship_created effect response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/effect.html
///     <seealso cref="Requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class SignerSponsorshipCreatedEffectResponse : EffectResponse
{
    public SignerSponsorshipCreatedEffectResponse()
    {
    }

    public SignerSponsorshipCreatedEffectResponse(string signer, string sponsor)
    {
        Signer = signer;
        Sponsor = sponsor;
    }

    public override int TypeId => 72;

    [JsonProperty(PropertyName = "signer")]
    public string Signer { get; private set; }

    [JsonProperty(PropertyName = "sponsor")]
    public string Sponsor { get; private set; }
}