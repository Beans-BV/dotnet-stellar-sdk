using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses.Results;

public class CreateClaimableBalanceResult : OperationResult
{
    public static CreateClaimableBalanceResult FromXdr(Xdr.CreateClaimableBalanceResult result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case CreateClaimableBalanceResultCode.CreateClaimableBalanceResultCodeEnum
                .CREATE_CLAIMABLE_BALANCE_LOW_RESERVE:
                return new CreateClaimableBalanceLowReserve();
            case CreateClaimableBalanceResultCode.CreateClaimableBalanceResultCodeEnum
                .CREATE_CLAIMABLE_BALANCE_MALFORMED:
                return new CreateClaimableBalanceMalformed();
            case CreateClaimableBalanceResultCode.CreateClaimableBalanceResultCodeEnum
                .CREATE_CLAIMABLE_BALANCE_NOT_AUTHORIZED:
                return new CreateClaimableBalanceNotAuthorized();
            case CreateClaimableBalanceResultCode.CreateClaimableBalanceResultCodeEnum
                .CREATE_CLAIMABLE_BALANCE_NO_TRUST:
                return new CreateClaimableBalanceNoTrust();
            case CreateClaimableBalanceResultCode.CreateClaimableBalanceResultCodeEnum.CREATE_CLAIMABLE_BALANCE_SUCCESS:
                return new CreateClaimableBalanceSuccess(result.BalanceID.V0.InnerValue);
            case CreateClaimableBalanceResultCode.CreateClaimableBalanceResultCodeEnum
                .CREATE_CLAIMABLE_BALANCE_UNDERFUNDED:
                return new CreateClaimableBalanceUnderfunded();
            default:
                throw new SystemException("Unknown CreateClaimableBalance type");
        }
    }
}