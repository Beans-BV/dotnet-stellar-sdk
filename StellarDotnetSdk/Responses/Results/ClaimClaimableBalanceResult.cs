using System;
using ResultCodeEnum = StellarDotnetSdk.Xdr.ClaimClaimableBalanceResultCode.ClaimClaimableBalanceResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class ClaimClaimableBalanceResult : OperationResult
{
    public static ClaimClaimableBalanceResult FromXdr(Xdr.ClaimClaimableBalanceResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_CANNOT_CLAIM => new ClaimClaimableBalanceCannotClaim(),
            ResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_DOES_NOT_EXIST => new ClaimClaimableBalanceDoesNotExist(),
            ResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_LINE_FULL => new ClaimClaimableBalanceLineFull(),
            ResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_NOT_AUTHORIZED => new ClaimClaimableBalanceNotAuthorized(),
            ResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_NO_TRUST => new ClaimClaimableBalanceNoTrust(),
            ResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_SUCCESS => new ClaimClaimableBalanceSuccess(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown ClaimClaimableBalanceResult type."),
        };
    }
}

public class ClaimClaimableBalanceSuccess : ClaimClaimableBalanceResult
{
    public override bool IsSuccess => true;
}

/// <summary>
///     There is no existing ClaimableBalanceEntry that matches the input BalanceID.
/// </summary>
public class ClaimClaimableBalanceDoesNotExist : ClaimClaimableBalanceResult;

/// <summary>
///     There is no claimant that matches the source account, or the claimants predicate is not satisfied.
/// </summary>
public class ClaimClaimableBalanceCannotClaim : ClaimClaimableBalanceResult;

/// <summary>
///     The account claiming the ClaimableBalanceEntry does not have sufficient limits to receive amount of the asset and
///     still satisfy its buying liabilities.
/// </summary>
public class ClaimClaimableBalanceLineFull : ClaimClaimableBalanceResult;

/// <summary>
///     The source account does not trust the issuer of the asset it is trying to claim in the ClaimableBalanceEntry.
/// </summary>
public class ClaimClaimableBalanceNoTrust : ClaimClaimableBalanceResult;

/// <summary>
///     The source account is not authorized to claim the asset in the ClaimableBalanceEntry.
/// </summary>
public class ClaimClaimableBalanceNotAuthorized : ClaimClaimableBalanceResult;