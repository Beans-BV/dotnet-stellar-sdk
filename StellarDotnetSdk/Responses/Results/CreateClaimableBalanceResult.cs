using System;
using ResultCodeEnum = StellarDotnetSdk.Xdr.CreateClaimableBalanceResultCode.CreateClaimableBalanceResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class CreateClaimableBalanceResult : OperationResult
{
    public static CreateClaimableBalanceResult FromXdr(Xdr.CreateClaimableBalanceResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.CREATE_CLAIMABLE_BALANCE_LOW_RESERVE
                => new CreateClaimableBalanceLowReserve(),
            ResultCodeEnum.CREATE_CLAIMABLE_BALANCE_MALFORMED
                => new CreateClaimableBalanceMalformed(),
            ResultCodeEnum.CREATE_CLAIMABLE_BALANCE_NOT_AUTHORIZED
                => new CreateClaimableBalanceNotAuthorized(),
            ResultCodeEnum.CREATE_CLAIMABLE_BALANCE_NO_TRUST
                => new CreateClaimableBalanceNoTrust(),
            ResultCodeEnum.CREATE_CLAIMABLE_BALANCE_SUCCESS
                => new CreateClaimableBalanceSuccess(ClaimableBalanceUtils.ToHexString(result.BalanceID)),
            ResultCodeEnum.CREATE_CLAIMABLE_BALANCE_UNDERFUNDED
                => new CreateClaimableBalanceUnderfunded(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown CreateClaimableBalanceResult type."),
        };
    }
}

public class CreateClaimableBalanceSuccess : CreateClaimableBalanceResult
{
    /// <summary>
    ///     Constructs a new <c>CreateClaimableBalanceSuccess</c> result.
    /// </summary>
    /// <param name="balanceId">A hex-encoded claimable balance ID (0000...).</param>
    public CreateClaimableBalanceSuccess(string balanceId)
    {
        if (!StrKey.IsValidClaimableBalanceId(ClaimableBalanceUtils.ToBase32String(balanceId)))
        {
            throw new ArgumentException($"Invalid claimable balance ID {balanceId}");
        }
        BalanceId = balanceId;
    }

    public override bool IsSuccess => true;

    /// <summary>
    ///     Hex-encoded claimable balance ID (0000...).
    /// </summary>
    public string BalanceId { get; }
}

/// <summary>
///     The input to this operation is invalid.
/// </summary>
public class CreateClaimableBalanceMalformed : CreateClaimableBalanceResult;

/// <summary>
///     The account creating this entry does not have enough XLM to satisfy the minimum XLM reserve increase caused by
///     adding a ClaimableBalanceEntry. For every claimant in the list, the minimum amount of XLM this account must hold
///     will increase by baseReserve.
/// </summary>
public class CreateClaimableBalanceLowReserve : CreateClaimableBalanceResult;

/// <summary>
///     The source account does not trust the issuer of the asset it is trying to include in the ClaimableBalanceEntry.
/// </summary>
public class CreateClaimableBalanceNoTrust : CreateClaimableBalanceResult;

/// <summary>
///     The source account is not authorized to transfer this asset.
/// </summary>
public class CreateClaimableBalanceNotAuthorized : CreateClaimableBalanceResult;

/// <summary>
///     The source account does not have enough funds to transfer amount of this asset to the ClaimableBalanceEntry.
/// </summary>
public class CreateClaimableBalanceUnderfunded : CreateClaimableBalanceResult;