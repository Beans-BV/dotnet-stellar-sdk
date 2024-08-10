using System;
using ResultCodeEnum = StellarDotnetSdk.Xdr.ClawbackClaimableBalanceResultCode.ClawbackClaimableBalanceResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class ClawbackClaimableBalanceResult : OperationResult
{
    public static ClawbackClaimableBalanceResult FromXdr(Xdr.ClawbackClaimableBalanceResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.CLAWBACK_CLAIMABLE_BALANCE_DOES_NOT_EXIST
                => new ClawbackClaimableBalanceDoesNotExist(),
            ResultCodeEnum.CLAWBACK_CLAIMABLE_BALANCE_NOT_CLAWBACK_ENABLED
                => new ClawbackClaimableBalanceNotClawbackEnabled(),
            ResultCodeEnum.CLAWBACK_CLAIMABLE_BALANCE_NOT_ISSUER
                => new ClawbackClaimableBalanceNotIssuer(),
            ResultCodeEnum.CLAWBACK_CLAIMABLE_BALANCE_SUCCESS
                => new ClawbackClaimableBalanceSuccess(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown ClawbackClaimableBalanceResult type."),
        };
    }
}

public class ClawbackClaimableBalanceSuccess : ClawbackClaimableBalanceResult
{
    public override bool IsSuccess => true;
}

/// <summary>
///     There is no existing ClaimableBalanceEntry that matches the input BalanceID.
/// </summary>
public class ClawbackClaimableBalanceDoesNotExist : ClawbackClaimableBalanceResult;

/// <summary>
///     The source account is not the issuer of the asset in the claimable balance.
/// </summary>
public class ClawbackClaimableBalanceNotIssuer : ClawbackClaimableBalanceResult;

/// <summary>
///     The CLAIMABLE_BALANCE_CLAWBACK_ENABLED_FLAG is not set for this trustline.
/// </summary>
public class ClawbackClaimableBalanceNotClawbackEnabled : ClawbackClaimableBalanceResult;