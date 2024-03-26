using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents trustline_sponsorship_removed effect response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/effect.html
///     <seealso cref="Requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class TrustlineSponsorshipRemovedEffectResponse : EffectResponse
{
    public TrustlineSponsorshipRemovedEffectResponse()
    {
    }

    public TrustlineSponsorshipRemovedEffectResponse(string asset, string formerSponsor)
    {
        Asset = asset;
        FormerSponsor = formerSponsor;
    }

    public override int TypeId => 65;

    [JsonProperty(PropertyName = "asset")] public string Asset { get; private set; }

    [JsonProperty(PropertyName = "former_sponsor")]
    public string FormerSponsor { get; private set; }
}