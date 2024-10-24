using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Xdr;
using LiquidityPoolParameters = StellarDotnetSdk.LiquidityPool.LiquidityPoolParameters;
using xdr_Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Withdraw assets from a liquidity pool, reducing the number of pool shares in exchange for reserves of a liquidity
///     pool.
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#liquidity-pool-withdraw">
///         Liquidity
///         pool withdraw
///     </a>
/// </summary>
public class LiquidityPoolWithdrawOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>LiquidityPoolWithdrawOperation</c>.
    /// </summary>
    /// <param name="liquidityPoolId">The Pool ID for the Liquidity Pool to withdraw from.</param>
    /// <param name="amount">Amount of pool shares to withdraw.</param>
    /// <param name="minAmountA">Minimum amount of the first asset to withdraw.</param>
    /// <param name="minAmountB">Minimum amount of the second asset to withdraw.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public LiquidityPoolWithdrawOperation(
        LiquidityPoolId liquidityPoolId,
        string amount,
        string minAmountA,
        string minAmountB,
        IAccountId? sourceAccount = null) : base(sourceAccount)
    {
        LiquidityPoolId = liquidityPoolId ??
                          throw new ArgumentNullException(nameof(liquidityPoolId), "liquidityPoolId cannot be null");
        Amount = amount ?? throw new ArgumentNullException(nameof(amount), "amount cannot be null");
        MinAmountA = minAmountA ?? throw new ArgumentNullException(nameof(minAmountA), "minAmountA cannot be null");
        MinAmountB = minAmountB ?? throw new ArgumentNullException(nameof(minAmountB), "minAmountB cannot be null");
    }


    public LiquidityPoolWithdrawOperation(
        AssetAmount assetA,
        AssetAmount assetB,
        string amount,
        IAccountId? sourceAccount = null)
        : this(new LiquidityPoolId(
                LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA.Asset,
                assetB.Asset,
                LiquidityPoolParameters.Fee),
            amount,
            assetA.Amount,
            assetB.Amount,
            sourceAccount)
    {
    }

    /// <summary>
    ///     The Pool ID for the Liquidity Pool to withdraw from.
    /// </summary>
    public LiquidityPoolId LiquidityPoolId { get; }

    /// <summary>
    ///     Amount of pool shares to withdraw.
    /// </summary>
    public string Amount { get; }

    /// <summary>
    ///     Minimum amount of the first asset to withdraw.
    /// </summary>
    public string MinAmountA { get; }

    /// <summary>
    ///     Minimum amount of the second asset to withdraw.
    /// </summary>
    public string MinAmountB { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        var body = new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.LIQUIDITY_POOL_WITHDRAW),
            LiquidityPoolWithdrawOp = new LiquidityPoolWithdrawOp
            {
                LiquidityPoolID = LiquidityPoolId.ToXdr(),
                Amount = new xdr_Int64(ToXdrAmount(Amount)),
                MinAmountA = new xdr_Int64(ToXdrAmount(MinAmountA)),
                MinAmountB = new xdr_Int64(ToXdrAmount(MinAmountB)),
            },
        };
        return body;
    }

    public static LiquidityPoolWithdrawOperation FromXdr(LiquidityPoolWithdrawOp liquidityPoolWithdrawOp)
    {
        return new LiquidityPoolWithdrawOperation(
            LiquidityPoolId.FromXdr(liquidityPoolWithdrawOp.LiquidityPoolID),
            StellarDotnetSdk.Amount.FromXdr(liquidityPoolWithdrawOp.Amount.InnerValue),
            StellarDotnetSdk.Amount.FromXdr(liquidityPoolWithdrawOp.MinAmountA.InnerValue),
            StellarDotnetSdk.Amount.FromXdr(liquidityPoolWithdrawOp.MinAmountB.InnerValue)
        );
    }
}