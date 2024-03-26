using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents trustline_sponsorship_created effect response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/effect.html
///     <seealso cref="Requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class TrustlineSponsorshipCreatedEffectResponse : EffectResponse
{
    public TrustlineSponsorshipCreatedEffectResponse()
    {
    }

    public TrustlineSponsorshipCreatedEffectResponse(string asset, string sponsor)
    {
        Asset = asset;
        Sponsor = sponsor;
    }

    public override int TypeId => 63;

    [JsonProperty(PropertyName = "asset")] public string Asset { get; private set; }

    [JsonProperty(PropertyName = "sponsor")]
    public string Sponsor { get; private set; }
}