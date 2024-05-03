using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Xdr;
using LiquidityPoolParameters = StellarDotnetSdk.LiquidityPool.LiquidityPoolParameters;
using xdr_Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Deposits assets into a liquidity pool, increasing the reserves of a liquidity pool in exchange for pool shares.
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#liquidity-pool-deposit">
///         Liquidity pool deposit
///     </a>
/// </summary>
public class LiquidityPoolDepositOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>LiquidityPoolDepositOperation</c>.
    /// </summary>
    /// <param name="liquidityPoolId">The PoolID for the Liquidity Pool to deposit into.</param>
    /// <param name="maxAmountA">Maximum amount of first asset to deposit.</param>
    /// <param name="maxAmountB">Maximum amount of second asset to deposit.</param>
    /// <param name="minPrice">Minimum depositA/depositB.</param>
    /// <param name="maxPrice">Maximum depositA/depositB.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public LiquidityPoolDepositOperation(
        LiquidityPoolID liquidityPoolId,
        string maxAmountA,
        string maxAmountB,
        Price minPrice,
        Price maxPrice, IAccountId? sourceAccount = null) : base(sourceAccount)
    {
        LiquidityPoolID = liquidityPoolId ??
                          throw new ArgumentNullException(nameof(liquidityPoolId), "liquidityPoolID cannot be null");
        MaxAmountA = maxAmountA ?? throw new ArgumentNullException(nameof(maxAmountA), "maxAmountA cannot be null");
        MaxAmountB = maxAmountB ?? throw new ArgumentNullException(nameof(maxAmountB), "maxAmountB cannot be null");
        MinPrice = minPrice ?? throw new ArgumentNullException(nameof(minPrice), "minPrice cannot be null");
        MaxPrice = maxPrice ?? throw new ArgumentNullException(nameof(maxPrice), "maxPrice cannot be null");
    }

    public LiquidityPoolDepositOperation(AssetAmount assetAmountA,
        AssetAmount assetAmountB,
        Price minPrice,
        Price maxPrice,
        IAccountId? sourceAccount = null)
        : this(new LiquidityPoolID(
                LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetAmountA.Asset,
                assetAmountB.Asset,
                LiquidityPoolParameters.Fee),
            assetAmountA.Amount,
            assetAmountB.Amount,
            minPrice,
            maxPrice,
            sourceAccount)
    {
    }

    /// <summary>
    ///     The PoolID for the Liquidity Pool to deposit into.
    /// </summary>
    public LiquidityPoolID LiquidityPoolID { get; }

    /// <summary>
    ///     Maximum amount of first asset to deposit.
    /// </summary>
    public string MaxAmountA { get; }

    /// <summary>
    ///     Maximum amount of second asset to deposit.
    /// </summary>
    public string MaxAmountB { get; }

    /// <summary>
    ///     Minimum depositA/depositB.
    /// </summary>
    public Price MinPrice { get; }

    /// <summary>
    ///     Maximum depositA/depositB.
    /// </summary>
    public Price MaxPrice { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.LIQUIDITY_POOL_DEPOSIT),
            LiquidityPoolDepositOp = new LiquidityPoolDepositOp
            {
                LiquidityPoolID = LiquidityPoolID.ToXdr(),
                MaxAmountA = new xdr_Int64(ToXdrAmount(MaxAmountA)),
                MaxAmountB = new xdr_Int64(ToXdrAmount(MaxAmountB)),
                MinPrice = MinPrice.ToXdr(),
                MaxPrice = MaxPrice.ToXdr()
            }
        };
    }

    public static LiquidityPoolDepositOperation FromXdr(LiquidityPoolDepositOp liquidityPoolDepositOp)
    {
        return new LiquidityPoolDepositOperation(
            LiquidityPoolID.FromXdr(liquidityPoolDepositOp.LiquidityPoolID),
            FromXdrAmount(liquidityPoolDepositOp.MaxAmountA.InnerValue),
            FromXdrAmount(liquidityPoolDepositOp.MaxAmountB.InnerValue),
            Price.FromXdr(liquidityPoolDepositOp.MinPrice),
            Price.FromXdr(liquidityPoolDepositOp.MaxPrice)
        );
    }
}