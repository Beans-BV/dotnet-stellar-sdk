namespace StellarDotnetSdk.Responses.Results;

/// <summary>
///     Offer deleted.
/// </summary>
public class ManageSellOfferDeleted : ManageSellOfferSuccess
{
    public ManageSellOfferDeleted(ClaimAtom[] offersClaimed) : base(offersClaimed)
    {
    }
}