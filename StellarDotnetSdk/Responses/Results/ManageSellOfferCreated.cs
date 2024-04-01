namespace StellarDotnetSdk.Responses.Results;

/// <summary>
///     Offer created.
/// </summary>
public class ManageSellOfferCreated : ManageSellOfferSuccess
{
    public ManageSellOfferCreated(OfferEntry offer, ClaimAtom[] offersClaimed) : base(offersClaimed)
    {
        Offer = offer;
    }

    /// <summary>
    ///     The offer that was created.
    /// </summary>
    public OfferEntry Offer { get; }
}