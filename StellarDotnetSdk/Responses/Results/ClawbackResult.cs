using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses.Results;

public class ClawbackResult : OperationResult
{
    public static ClawbackResult FromXdr(Xdr.ClawbackResult result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_MALFORMED:
                return new ClawbackMalformed();
            case ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_NOT_CLAWBACK_ENABLED:
                return new ClawbackNotClawbackEnabled();
            case ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_NO_TRUST:
                return new ClawbackNoTrust();
            case ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_SUCCESS:
                return new ClawbackSuccess();
            case ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_UNDERFUNDED:
                return new ClawbackUnderfunded();
            default:
                throw new SystemException("Unknown ClawbackResult type");
        }
    }
}