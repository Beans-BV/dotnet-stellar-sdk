using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses.Results;

public class ClaimClaimableBalanceResult : OperationResult
{
    public static ClaimClaimableBalanceResult FromXdr(Xdr.ClaimClaimableBalanceResult result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case ClaimClaimableBalanceResultCode.ClaimClaimableBalanceResultCodeEnum
                .CLAIM_CLAIMABLE_BALANCE_CANNOT_CLAIM:
                return new ClaimClaimableBalanceCannotClaim();
            case ClaimClaimableBalanceResultCode.ClaimClaimableBalanceResultCodeEnum
                .CLAIM_CLAIMABLE_BALANCE_DOES_NOT_EXIST:
                return new ClaimClaimableBalanceDoesNotExist();
            case ClaimClaimableBalanceResultCode.ClaimClaimableBalanceResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_LINE_FULL:
                return new ClaimClaimableBalanceLineFull();
            case ClaimClaimableBalanceResultCode.ClaimClaimableBalanceResultCodeEnum
                .CLAIM_CLAIMABLE_BALANCE_NOT_AUTHORIZED:
                return new ClaimClaimableBalanceNotAuthorized();
            case ClaimClaimableBalanceResultCode.ClaimClaimableBalanceResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_NO_TRUST:
                return new ClaimClaimableBalanceNoTrust();
            case ClaimClaimableBalanceResultCode.ClaimClaimableBalanceResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_SUCCESS:
                return new ClaimClaimableBalanceSuccess();
            default:
                throw new SystemException("Unknown ClaimClaimableBalance type");
        }
    }
}