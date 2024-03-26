using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents account_sponsorship_removed effect response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/effect.html
///     <seealso cref="Requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class AccountSponsorshipRemovedEffectResponse : EffectResponse
{
    public AccountSponsorshipRemovedEffectResponse()
    {
    }

    public AccountSponsorshipRemovedEffectResponse(string formerSponsor)
    {
        FormerSponsor = formerSponsor;
    }

    public override int TypeId => 62;

    [JsonProperty(PropertyName = "former_sponsor")]
    public string FormerSponsor { get; private set; }
}