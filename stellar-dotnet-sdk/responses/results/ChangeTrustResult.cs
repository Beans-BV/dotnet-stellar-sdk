using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk.responses.results;

public class ChangeTrustResult : OperationResult
{
    public static ChangeTrustResult FromXdr(xdr.ChangeTrustResult result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case ChangeTrustResultCode.ChangeTrustResultCodeEnum.CHANGE_TRUST_SUCCESS:
                return new ChangeTrustSuccess();
            case ChangeTrustResultCode.ChangeTrustResultCodeEnum.CHANGE_TRUST_MALFORMED:
                return new ChangeTrustMalformed();
            case ChangeTrustResultCode.ChangeTrustResultCodeEnum.CHANGE_TRUST_NO_ISSUER:
                return new ChangeTrustNoIssuer();
            case ChangeTrustResultCode.ChangeTrustResultCodeEnum.CHANGE_TRUST_INVALID_LIMIT:
                return new ChangeTrustInvalidLimit();
            case ChangeTrustResultCode.ChangeTrustResultCodeEnum.CHANGE_TRUST_LOW_RESERVE:
                return new ChangeTrustLowReserve();
            case ChangeTrustResultCode.ChangeTrustResultCodeEnum.CHANGE_TRUST_SELF_NOT_ALLOWED:
                return new ChangeTrustSelfNotAllowed();
            default:
                throw new SystemException("Unknown ChangeTrust type");
        }
    }
}