using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses.Results;

public class ClawbackClaimableBalanceResult : OperationResult
{
    public static ClawbackClaimableBalanceResult FromXdr(Xdr.ClawbackClaimableBalanceResult result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case ClawbackClaimableBalanceResultCode.ClawbackClaimableBalanceResultCodeEnum
                .CLAWBACK_CLAIMABLE_BALANCE_DOES_NOT_EXIST:
                return new ClawbackClaimableBalanceDoesNotExist();
            case ClawbackClaimableBalanceResultCode.ClawbackClaimableBalanceResultCodeEnum
                .CLAWBACK_CLAIMABLE_BALANCE_NOT_CLAWBACK_ENABLED:
                return new ClawbackClaimableBalanceNotClawbackEnabled();
            case ClawbackClaimableBalanceResultCode.ClawbackClaimableBalanceResultCodeEnum
                .CLAWBACK_CLAIMABLE_BALANCE_NOT_ISSUER:
                return new ClawbackClaimableBalanceNotIssuer();
            case ClawbackClaimableBalanceResultCode.ClawbackClaimableBalanceResultCodeEnum
                .CLAWBACK_CLAIMABLE_BALANCE_SUCCESS:
                return new ClawbackClaimableBalanceSuccess();
            default:
                throw new SystemException("Unknown ClawbackClaimableBalance type");
        }
    }
}