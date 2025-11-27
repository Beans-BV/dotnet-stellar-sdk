namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the offer_updated effect response.
///     This effect occurs when an offer is updated on the DEX.
/// </summary>
public sealed class OfferUpdatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 32;
}