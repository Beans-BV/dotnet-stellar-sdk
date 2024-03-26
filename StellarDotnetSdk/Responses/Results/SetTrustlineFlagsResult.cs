using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses.Results;

public class SetTrustlineFlagsResult : OperationResult
{
    public static SetTrustlineFlagsResult FromXdr(SetTrustLineFlagsResult result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case SetTrustLineFlagsResultCode.SetTrustLineFlagsResultCodeEnum.SET_TRUST_LINE_FLAGS_CANT_REVOKE:
                return new SetTrustlineFlagsCantRevoke();
            case SetTrustLineFlagsResultCode.SetTrustLineFlagsResultCodeEnum.SET_TRUST_LINE_FLAGS_INVALID_STATE:
                return new SetTrustlineFlagsInvalidState();
            case SetTrustLineFlagsResultCode.SetTrustLineFlagsResultCodeEnum.SET_TRUST_LINE_FLAGS_MALFORMED:
                return new SetTrustlineFlagsMalformed();
            case SetTrustLineFlagsResultCode.SetTrustLineFlagsResultCodeEnum.SET_TRUST_LINE_FLAGS_NO_TRUST_LINE:
                return new SetTrustlineFlagsNoTrustline();
            case SetTrustLineFlagsResultCode.SetTrustLineFlagsResultCodeEnum.SET_TRUST_LINE_FLAGS_SUCCESS:
                return new SetTrustlineFlagsSuccess();
            default:
                throw new SystemException("Unknown SetTrustlineFlagsResult type");
        }
    }
}