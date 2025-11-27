namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the offer_created effect response.
///     This effect occurs when a new offer is created on the DEX.
/// </summary>
public sealed class OfferCreatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 30;
}