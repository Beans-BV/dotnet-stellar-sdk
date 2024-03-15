namespace stellar_dotnet_sdk.responses.results;

/// <summary>
///     Offer deleted.
/// </summary>
public class ManageSellOfferDeleted : ManageSellOfferSuccess
{
    public ManageSellOfferDeleted(ClaimAtom[] offersClaimed) : base(offersClaimed)
    {
    }
}