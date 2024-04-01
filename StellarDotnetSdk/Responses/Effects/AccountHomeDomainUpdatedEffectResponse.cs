using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents account_home_domain_updated effect response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/effect.html
///     <seealso cref="Requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class AccountHomeDomainUpdatedEffectResponse : EffectResponse
{
    public AccountHomeDomainUpdatedEffectResponse()
    {
    }

    /// <inheritdoc />
    public AccountHomeDomainUpdatedEffectResponse(string homeDomain)
    {
        HomeDomain = homeDomain;
    }

    [JsonProperty(PropertyName = "home_domain")]
    public string HomeDomain { get; private set; }

    public override int TypeId => 5;
}