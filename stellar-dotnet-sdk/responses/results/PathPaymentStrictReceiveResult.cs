using System;
using System.Linq;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk.responses.results;

public class PathPaymentStrictReceiveResult : OperationResult
{
    public static PathPaymentStrictReceiveResult FromXdr(xdr.PathPaymentStrictReceiveResult result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case PathPaymentStrictReceiveResultCode.PathPaymentStrictReceiveResultCodeEnum
                .PATH_PAYMENT_STRICT_RECEIVE_SUCCESS:
                return new PathPaymentStrictReceiveSuccess
                {
                    Offers = OffersFromXdr(result.Success.Offers),
                    Last = PathPaymentStrictReceiveSuccess.SimplePaymentResult.FromXdr(result.Success.Last)
                };
            case PathPaymentStrictReceiveResultCode.PathPaymentStrictReceiveResultCodeEnum
                .PATH_PAYMENT_STRICT_RECEIVE_MALFORMED:
                return new PathPaymentStrictReceiveMalformed();
            case PathPaymentStrictReceiveResultCode.PathPaymentStrictReceiveResultCodeEnum
                .PATH_PAYMENT_STRICT_RECEIVE_UNDERFUNDED:
                return new PathPaymentStrictReceiveUnderfunded();
            case PathPaymentStrictReceiveResultCode.PathPaymentStrictReceiveResultCodeEnum
                .PATH_PAYMENT_STRICT_RECEIVE_SRC_NO_TRUST:
                return new PathPaymentStrictReceiveSrcNoTrust();
            case PathPaymentStrictReceiveResultCode.PathPaymentStrictReceiveResultCodeEnum
                .PATH_PAYMENT_STRICT_RECEIVE_SRC_NOT_AUTHORIZED:
                return new PathPaymentStrictReceiveSrcNotAuthorized();
            case PathPaymentStrictReceiveResultCode.PathPaymentStrictReceiveResultCodeEnum
                .PATH_PAYMENT_STRICT_RECEIVE_NO_DESTINATION:
                return new PathPaymentStrictReceiveNoDestination();
            case PathPaymentStrictReceiveResultCode.PathPaymentStrictReceiveResultCodeEnum
                .PATH_PAYMENT_STRICT_RECEIVE_NO_TRUST:
                return new PathPaymentStrictReceiveNoTrust();
            case PathPaymentStrictReceiveResultCode.PathPaymentStrictReceiveResultCodeEnum
                .PATH_PAYMENT_STRICT_RECEIVE_NOT_AUTHORIZED:
                return new PathPaymentStrictReceiveNotAuthorized();
            case PathPaymentStrictReceiveResultCode.PathPaymentStrictReceiveResultCodeEnum
                .PATH_PAYMENT_STRICT_RECEIVE_LINE_FULL:
                return new PathPaymentStrictReceiveLineFull();
            case PathPaymentStrictReceiveResultCode.PathPaymentStrictReceiveResultCodeEnum
                .PATH_PAYMENT_STRICT_RECEIVE_NO_ISSUER:
                return new PathPaymentStrictReceiveNoIssuer
                {
                    NoIssuer = Asset.FromXdr(result.NoIssuer)
                };
            case PathPaymentStrictReceiveResultCode.PathPaymentStrictReceiveResultCodeEnum
                .PATH_PAYMENT_STRICT_RECEIVE_TOO_FEW_OFFERS:
                return new PathPaymentStrictReceiveTooFewOffers();
            case PathPaymentStrictReceiveResultCode.PathPaymentStrictReceiveResultCodeEnum
                .PATH_PAYMENT_STRICT_RECEIVE_OFFER_CROSS_SELF:
                return new PathPaymentStrictReceiveOfferCrossSelf();
            case PathPaymentStrictReceiveResultCode.PathPaymentStrictReceiveResultCodeEnum
                .PATH_PAYMENT_STRICT_RECEIVE_OVER_SENDMAX:
                return new PathPaymentStrictReceiveOverSendmax();
            default:
                throw new SystemException("Unknown PathPayment type");
        }
    }

    private static ClaimAtom[] OffersFromXdr(xdr.ClaimAtom[] offers)
    {
        return offers.Select(ClaimAtom.FromXdr).ToArray();
    }
}