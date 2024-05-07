using System;
using ResultCodeEnum = StellarDotnetSdk.Xdr.LiquidityPoolWithdrawResultCode.LiquidityPoolWithdrawResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class LiquidityPoolWithdrawResult : OperationResult
{
    public static LiquidityPoolWithdrawResult FromXdr(Xdr.LiquidityPoolWithdrawResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.LIQUIDITY_POOL_WITHDRAW_SUCCESS => new LiquidityPoolWithdrawSuccess(),
            ResultCodeEnum.LIQUIDITY_POOL_WITHDRAW_MALFORMED => new LiquidityPoolWithdrawMalformed(),
            ResultCodeEnum.LIQUIDITY_POOL_WITHDRAW_NO_TRUST => new LiquidityPoolWithdrawNoTrust(),
            ResultCodeEnum.LIQUIDITY_POOL_WITHDRAW_UNDERFUNDED => new LiquidityPoolWithdrawUnderfunded(),
            ResultCodeEnum.LIQUIDITY_POOL_WITHDRAW_LINE_FULL => new LiquidityPoolWithdrawLineFull(),
            ResultCodeEnum.LIQUIDITY_POOL_WITHDRAW_UNDER_MINIMUM => new LiquidityPoolWithdrawUnderMinimum(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown LiquidityPoolWithdrawResult type.")
        };
    }
}

public class LiquidityPoolWithdrawSuccess : LiquidityPoolWithdrawResult
{
    public override bool IsSuccess => true;
}

/// <summary>
///     One or more of the inputs to the operation was malformed.
/// </summary>
public class LiquidityPoolWithdrawMalformed : LiquidityPoolWithdrawResult;

/// <summary>
///     There is no trustline for one of the assets.
/// </summary>
public class LiquidityPoolWithdrawNoTrust : LiquidityPoolWithdrawResult;

/// <summary>
///     Insufficient balance for the pool shares.
/// </summary>
public class LiquidityPoolWithdrawUnderfunded : LiquidityPoolWithdrawResult;

/// <summary>
///     The withdrawal would exceed the trustline limit for one of the assets.
/// </summary>
public class LiquidityPoolWithdrawLineFull : LiquidityPoolWithdrawResult;

/// <summary>
///     Unable to withdraw enough to satisfy the minimum price.
/// </summary>
public class LiquidityPoolWithdrawUnderMinimum : LiquidityPoolWithdrawResult;