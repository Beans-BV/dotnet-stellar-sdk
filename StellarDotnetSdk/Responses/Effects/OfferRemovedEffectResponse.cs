namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the offer_removed effect response.
///     This effect occurs when an offer is removed from the DEX.
/// </summary>
public sealed class OfferRemovedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 31;
}