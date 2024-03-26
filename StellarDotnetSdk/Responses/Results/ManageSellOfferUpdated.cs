namespace StellarDotnetSdk.Responses.Results;

/// <summary>
///     Offer updated.
/// </summary>
public class ManageSellOfferUpdated : ManageSellOfferSuccess
{
    public ManageSellOfferUpdated(OfferEntry offer, ClaimAtom[] offersClaimed) : base(offersClaimed)
    {
        Offer = offer;
    }

    /// <summary>
    ///     The offer that was updated.
    /// </summary>
    public OfferEntry Offer { get; }
}