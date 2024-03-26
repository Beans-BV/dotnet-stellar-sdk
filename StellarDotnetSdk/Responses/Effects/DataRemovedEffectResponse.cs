namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents account_home_domain_updated effect response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/effect.html
///     <seealso cref="Requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class DataRemovedEffectResponse : EffectResponse
{
    public override int TypeId => 41;
}