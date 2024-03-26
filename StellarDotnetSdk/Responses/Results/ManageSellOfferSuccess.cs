using System;
using System.Linq;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses.Results;

/// <summary>
///     Operation successful.
/// </summary>
public class ManageSellOfferSuccess : ManageSellOfferResult
{
    protected ManageSellOfferSuccess(ClaimAtom[] offersClaimed)
    {
        OffersClaimed = offersClaimed;
    }

    public override bool IsSuccess => true;

    /// <summary>
    ///     Offers that got claimed while creating this offer.
    /// </summary>
    public ClaimAtom[] OffersClaimed { get; }

    public static ManageSellOfferSuccess FromXdr(ManageOfferSuccessResult result)
    {
        var offersClaimed = result.OffersClaimed.Select(ClaimAtom.FromXdr).ToArray();

        switch (result.Offer.Discriminant.InnerValue)
        {
            case ManageOfferEffect.ManageOfferEffectEnum.MANAGE_OFFER_CREATED:
                var createdOffer = OfferEntry.FromXdr(result.Offer.Offer);
                return new ManageSellOfferCreated(createdOffer, offersClaimed);
            case ManageOfferEffect.ManageOfferEffectEnum.MANAGE_OFFER_UPDATED:
                var updatedOffer = OfferEntry.FromXdr(result.Offer.Offer);
                return new ManageSellOfferUpdated(updatedOffer, offersClaimed);
            case ManageOfferEffect.ManageOfferEffectEnum.MANAGE_OFFER_DELETED:
                return new ManageSellOfferDeleted(offersClaimed);
            default:
                throw new SystemException("Unknown ManageSellOfferSuccess type");
        }
    }
}