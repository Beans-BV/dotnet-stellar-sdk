using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk.responses.results;

public class PaymentResult : OperationResult
{
    public static PaymentResult FromXdr(xdr.PaymentResult result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case PaymentResultCode.PaymentResultCodeEnum.PAYMENT_SUCCESS:
                return new PaymentSuccess();
            case PaymentResultCode.PaymentResultCodeEnum.PAYMENT_MALFORMED:
                return new PaymentMalformed();
            case PaymentResultCode.PaymentResultCodeEnum.PAYMENT_UNDERFUNDED:
                return new PaymentUnderfunded();
            case PaymentResultCode.PaymentResultCodeEnum.PAYMENT_SRC_NO_TRUST:
                return new PaymentSrcNoTrust();
            case PaymentResultCode.PaymentResultCodeEnum.PAYMENT_SRC_NOT_AUTHORIZED:
                return new PaymentSrcNotAuthorized();
            case PaymentResultCode.PaymentResultCodeEnum.PAYMENT_NO_DESTINATION:
                return new PaymentNoDestination();
            case PaymentResultCode.PaymentResultCodeEnum.PAYMENT_NO_TRUST:
                return new PaymentNoTrust();
            case PaymentResultCode.PaymentResultCodeEnum.PAYMENT_NOT_AUTHORIZED:
                return new PaymentNotAuthorized();
            case PaymentResultCode.PaymentResultCodeEnum.PAYMENT_LINE_FULL:
                return new PaymentLineFull();
            case PaymentResultCode.PaymentResultCodeEnum.PAYMENT_NO_ISSUER:
                return new PaymentNoIssuer();
            default:
                throw new SystemException("Unknown PaymentResult type");
        }
    }
}