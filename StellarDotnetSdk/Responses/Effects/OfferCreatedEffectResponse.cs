namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents offer_created effect response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/effect.html
///     <seealso cref="Requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class OfferCreatedEffectResponse : EffectResponse
{
    public override int TypeId => 30;
}