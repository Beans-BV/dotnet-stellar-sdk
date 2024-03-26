using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents account_sponsorship_created effect response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/effect.html
///     <seealso cref="Requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class AccountSponsorshipCreatedEffectResponse : EffectResponse
{
    public AccountSponsorshipCreatedEffectResponse()
    {
    }

    public AccountSponsorshipCreatedEffectResponse(string sponsor)
    {
        Sponsor = sponsor;
    }

    public override int TypeId => 60;

    [JsonProperty(PropertyName = "sponsor")]
    public string Sponsor { get; private set; }
}