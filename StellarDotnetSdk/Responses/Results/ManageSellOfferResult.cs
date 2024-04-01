using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses.Results;

public class ManageSellOfferResult : OperationResult
{
    public static ManageSellOfferResult FromXdr(Xdr.ManageSellOfferResult result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case ManageSellOfferResultCode.ManageSellOfferResultCodeEnum.MANAGE_SELL_OFFER_SUCCESS:
                return ManageSellOfferSuccess.FromXdr(result.Success);
            case ManageSellOfferResultCode.ManageSellOfferResultCodeEnum.MANAGE_SELL_OFFER_MALFORMED:
                return new ManageSellOfferMalformed();
            case ManageSellOfferResultCode.ManageSellOfferResultCodeEnum.MANAGE_SELL_OFFER_UNDERFUNDED:
                return new ManageSellOfferUnderfunded();
            case ManageSellOfferResultCode.ManageSellOfferResultCodeEnum.MANAGE_SELL_OFFER_SELL_NO_TRUST:
                return new ManageSellOfferSellNoTrust();
            case ManageSellOfferResultCode.ManageSellOfferResultCodeEnum.MANAGE_SELL_OFFER_BUY_NO_TRUST:
                return new ManageSellOfferBuyNoTrust();
            case ManageSellOfferResultCode.ManageSellOfferResultCodeEnum.MANAGE_SELL_OFFER_SELL_NOT_AUTHORIZED:
                return new ManageSellOfferSellNotAuthorized();
            case ManageSellOfferResultCode.ManageSellOfferResultCodeEnum.MANAGE_SELL_OFFER_BUY_NOT_AUTHORIZED:
                return new ManageSellOfferBuyNotAuthorized();
            case ManageSellOfferResultCode.ManageSellOfferResultCodeEnum.MANAGE_SELL_OFFER_LINE_FULL:
                return new ManageSellOfferLineFull();
            case ManageSellOfferResultCode.ManageSellOfferResultCodeEnum.MANAGE_SELL_OFFER_CROSS_SELF:
                return new ManageSellOfferCrossSelf();
            case ManageSellOfferResultCode.ManageSellOfferResultCodeEnum.MANAGE_SELL_OFFER_SELL_NO_ISSUER:
                return new ManageSellOfferSellNoIssuer();
            case ManageSellOfferResultCode.ManageSellOfferResultCodeEnum.MANAGE_SELL_OFFER_BUY_NO_ISSUER:
                return new ManageSellOfferBuyNoIssuer();
            case ManageSellOfferResultCode.ManageSellOfferResultCodeEnum.MANAGE_SELL_OFFER_NOT_FOUND:
                return new ManageSellOfferNotFound();
            case ManageSellOfferResultCode.ManageSellOfferResultCodeEnum.MANAGE_SELL_OFFER_LOW_RESERVE:
                return new ManageSellOfferLowReserve();
            default:
                throw new SystemException("Unknown ManageOffer type");
        }
    }
}