using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk.responses.results;

public class AllowTrustResult : OperationResult
{
    public static AllowTrustResult FromXdr(xdr.AllowTrustResult result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_SUCCESS:
                return new AllowTrustSuccess();
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_MALFORMED:
                return new AllowTrustMalformed();
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_NO_TRUST_LINE:
                return new AllowTrustNoTrustline();
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_TRUST_NOT_REQUIRED:
                return new AllowTrustNotRequired();
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_CANT_REVOKE:
                return new AllowTrustCantRevoke();
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_SELF_NOT_ALLOWED:
                return new AllowTrustSelfNotAllowed();
            default:
                throw new SystemException("Unknown AllowTrust type");
        }
    }
}