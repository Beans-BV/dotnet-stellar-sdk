using System;
using ResultCodeEnum = StellarDotnetSdk.Xdr.LiquidityPoolDepositResultCode.LiquidityPoolDepositResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class LiquidityPoolDepositResult : OperationResult
{
    public static LiquidityPoolDepositResult FromXdr(Xdr.LiquidityPoolDepositResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.LIQUIDITY_POOL_DEPOSIT_SUCCESS => new LiquidityPoolDepositSuccess(),
            ResultCodeEnum.LIQUIDITY_POOL_DEPOSIT_MALFORMED => new LiquidityPoolDepositMalformed(),
            ResultCodeEnum.LIQUIDITY_POOL_DEPOSIT_NO_TRUST => new LiquidityPoolDepositNoTrust(),
            ResultCodeEnum.LIQUIDITY_POOL_DEPOSIT_NOT_AUTHORIZED => new LiquidityPoolDepositNotAuthorized(),
            ResultCodeEnum.LIQUIDITY_POOL_DEPOSIT_UNDERFUNDED => new LiquidityPoolDepositUnderfunded(),
            ResultCodeEnum.LIQUIDITY_POOL_DEPOSIT_LINE_FULL => new LiquidityPoolDepositLineFull(),
            ResultCodeEnum.LIQUIDITY_POOL_DEPOSIT_BAD_PRICE => new LiquidityPoolDepositBadPrice(),
            ResultCodeEnum.LIQUIDITY_POOL_DEPOSIT_POOL_FULL => new LiquidityPoolDepositPoolFull(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown LiquidityPoolDepositResult type.")
        };
    }
}

public class LiquidityPoolDepositSuccess : LiquidityPoolDepositResult
{
    public override bool IsSuccess => true;
}

/// <summary>
///     One or more of the inputs to the operation was malformed.
/// </summary>
public class LiquidityPoolDepositMalformed : LiquidityPoolDepositResult;

/// <summary>
///     No trustline exists for one of the assets being deposited.
/// </summary>
public class LiquidityPoolDepositNoTrust : LiquidityPoolDepositResult;

/// <summary>
///     The account does not have authorization for one of the assets.
/// </summary>
public class LiquidityPoolDepositNotAuthorized : LiquidityPoolDepositResult;

/// <summary>
///     There is not enough balance of one of the assets to perform the deposit.
/// </summary>
public class LiquidityPoolDepositUnderfunded : LiquidityPoolDepositResult;

/// <summary>
///     The pool share trustline does not have a sufficient limit.
/// </summary>
public class LiquidityPoolDepositLineFull : LiquidityPoolDepositResult;

/// <summary>
///     The deposit price is outside of the given bounds.
/// </summary>
public class LiquidityPoolDepositBadPrice : LiquidityPoolDepositResult;

/// <summary>
///     The liquidity pool reserves are full.
/// </summary>
public class LiquidityPoolDepositPoolFull : LiquidityPoolDepositResult;