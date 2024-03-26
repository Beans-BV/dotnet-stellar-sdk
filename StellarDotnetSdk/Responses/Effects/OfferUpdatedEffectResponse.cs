namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents offer_updated effect response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/effect.html
///     <seealso cref="Requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class OfferUpdatedEffectResponse : EffectResponse
{
    public override int TypeId => 32;
}