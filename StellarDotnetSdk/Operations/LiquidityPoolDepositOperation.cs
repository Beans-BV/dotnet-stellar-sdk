using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Xdr;
using Assets_Asset = StellarDotnetSdk.Assets.Asset;
using xdr_Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Operations;

/// <summary>
/// Deposits assets into a liquidity pool, increasing the reserves of a liquidity pool in exchange for pool shares.
///     <p>Use <see cref="Builder" /> to to create a new <c>LiquidityPoolDepositOperation</c>.</p>
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#liquidity-pool-deposit">
///         Liquidity pool deposit
///     </a>
/// </summary>
public class LiquidityPoolDepositOperation : Operation
{
    private LiquidityPoolDepositOperation(
        LiquidityPoolID liquidityPoolID,
        string maxAmountA,
        string maxAmountB,
        Price minPrice,
        Price maxPrice)
    {
        LiquidityPoolID = liquidityPoolID ??
                          throw new ArgumentNullException(nameof(liquidityPoolID), "liquidityPoolID cannot be null");
        MaxAmountA = maxAmountA ?? throw new ArgumentNullException(nameof(maxAmountA), "maxAmountA cannot be null");
        MaxAmountB = maxAmountB ?? throw new ArgumentNullException(nameof(maxAmountB), "maxAmountB cannot be null");
        MinPrice = minPrice ?? throw new ArgumentNullException(nameof(minPrice), "minPrice cannot be null");
        MaxPrice = maxPrice ?? throw new ArgumentNullException(nameof(maxPrice), "maxPrice cannot be null");
    }

    /// <summary>
    /// The PoolID for the Liquidity Pool to deposit into.
    /// </summary>
    public LiquidityPoolID LiquidityPoolID { get; }
    
    /// <summary>
    /// Maximum amount of first asset to deposit.
    /// </summary>
    public string MaxAmountA { get; }
    
    /// <summary>
    /// Maximum amount of second asset to deposit.
    /// </summary>
    public string MaxAmountB { get; }
    
    /// <summary>
    /// Minimum depositA/depositB.
    /// </summary>
    public Price MinPrice { get; }
    
    /// <summary>
    /// Maximum depositA/depositB.
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

    /// <summary>
    ///     Builder for <c>LiquidityPoolDepositOperation</c>.
    /// </summary>
    public class Builder
    {
        private readonly Assets_Asset? _assetA;
        private readonly Assets_Asset? _assetB;
        private readonly LiquidityPoolID _liquidityPoolID;
        private readonly string _maxAmountA;
        private readonly string _maxAmountB;
        private readonly Price _maxPrice;
        private readonly Price _minPrice;
        private KeyPair? _sourceAccount;

        /// <summary>
        ///     Constructs a new <c>LiquidityPoolDepositOperation</c> builder.
        /// </summary>
        /// <param name="liquidityPoolDepositOp">A <c>LiquidityPoolDepositOp</c> XDR object.</param>
        public Builder(LiquidityPoolDepositOp liquidityPoolDepositOp)
        {
            _liquidityPoolID = LiquidityPoolID.FromXdr(liquidityPoolDepositOp.LiquidityPoolID);
            _maxAmountA = FromXdrAmount(liquidityPoolDepositOp.MaxAmountA.InnerValue);
            _maxAmountB = FromXdrAmount(liquidityPoolDepositOp.MaxAmountB.InnerValue);
            _minPrice = Price.FromXdr(liquidityPoolDepositOp.MinPrice);
            _maxPrice = Price.FromXdr(liquidityPoolDepositOp.MaxPrice);
        }

        public Builder(
            AssetAmount assetAmountA,
            AssetAmount assetAmountB,
            Price minPrice,
            Price maxPrice)
        {
            _liquidityPoolID =
                new LiquidityPoolID(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                    assetAmountA.Asset, assetAmountB.Asset, LiquidityPoolParameters.Fee);
            _maxAmountA = assetAmountA.Amount;
            _maxAmountB = assetAmountB.Amount;
            _minPrice = minPrice;
            _maxPrice = maxPrice;
        }

        public Builder(
            LiquidityPoolID liquidityPoolID,
            string maxAmountA,
            string maxAmountB,
            Price minPrice,
            Price maxPrice)
        {
            _liquidityPoolID = liquidityPoolID;
            _maxAmountA = maxAmountA;
            _maxAmountB = maxAmountB;
            _minPrice = minPrice;
            _maxPrice = maxPrice;
        }

        /// <summary>
        ///     Sets source account of this operation.
        /// </summary>
        /// <param name="sourceAccount">Source account</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetSourceAccount(KeyPair sourceAccount)
        {
            _sourceAccount = sourceAccount;
            return this;
        }

        /// <summary>
        /// Builds an operation.
        /// </summary>
        /// <returns></returns>
        public LiquidityPoolDepositOperation Build()
        {
            var operation =
                new LiquidityPoolDepositOperation(_liquidityPoolID, _maxAmountA, _maxAmountB, _minPrice, _maxPrice);

            if (_sourceAccount != null) operation.SourceAccount = _sourceAccount;

            return operation;
        }
    }
}