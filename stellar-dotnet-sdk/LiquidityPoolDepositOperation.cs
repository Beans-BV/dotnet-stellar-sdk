using System;
using stellar_dotnet_sdk.xdr;
using Int64 = stellar_dotnet_sdk.xdr.Int64;

namespace stellar_dotnet_sdk;

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

    public LiquidityPoolID LiquidityPoolID { get; }
    public string MaxAmountA { get; }
    public string MaxAmountB { get; }
    public Price MinPrice { get; }
    public Price MaxPrice { get; }

    public override xdr.Operation.OperationBody ToOperationBody()
    {
        var body = new xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.LIQUIDITY_POOL_DEPOSIT),
            LiquidityPoolDepositOp = new LiquidityPoolDepositOp
            {
                LiquidityPoolID = LiquidityPoolID.ToXdr(),
                MaxAmountA = new Int64(ToXdrAmount(MaxAmountA)),
                MaxAmountB = new Int64(ToXdrAmount(MaxAmountB)),
                MinPrice = MinPrice.ToXdr(),
                MaxPrice = MaxPrice.ToXdr()
            }
        };
        return body;
    }

    public class Builder
    {
        private readonly Asset? _assetA;
        private readonly Asset? _assetB;
        private readonly string _maxAmountA;
        private readonly string _maxAmountB;
        private readonly Price _maxPrice;
        private readonly Price _minPrice;
        private readonly LiquidityPoolID? _liquidityPoolID;

        private KeyPair? _sourceAccount;

        public Builder(LiquidityPoolDepositOp operationXdr)
        {
            _liquidityPoolID = LiquidityPoolID.FromXdr(operationXdr.LiquidityPoolID);
            _maxAmountA = FromXdrAmount(operationXdr.MaxAmountA.InnerValue);
            _maxAmountB = FromXdrAmount(operationXdr.MaxAmountB.InnerValue);
            _minPrice = Price.FromXdr(operationXdr.MinPrice);
            _maxPrice = Price.FromXdr(operationXdr.MaxPrice);
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

        public LiquidityPoolDepositOperation Build()
        {
            var operation =
                new LiquidityPoolDepositOperation(_liquidityPoolID, _maxAmountA, _maxAmountB, _minPrice, _maxPrice);

            if (_sourceAccount != null) operation.SourceAccount = _sourceAccount;

            return operation;
        }
    }
}