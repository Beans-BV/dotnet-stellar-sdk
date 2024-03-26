using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Xdr;
using xdr_Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Operations;

public class LiquidityPoolWithdrawOperation : Operation
{
    private LiquidityPoolWithdrawOperation(
        LiquidityPoolID liquidityPoolID,
        string amount,
        string minAmountA,
        string minAmountB)
    {
        LiquidityPoolID = liquidityPoolID ??
                          throw new ArgumentNullException(nameof(liquidityPoolID), "liquidityPoolID cannot be null");
        Amount = amount ?? throw new ArgumentNullException(nameof(amount), "amount cannot be null");
        MinAmountA = minAmountA ?? throw new ArgumentNullException(nameof(minAmountA), "minAmountA cannot be null");
        MinAmountB = minAmountB ?? throw new ArgumentNullException(nameof(minAmountB), "minAmountB cannot be null");
    }

    public LiquidityPoolID LiquidityPoolID { get; }
    public string Amount { get; }
    public string MinAmountA { get; }
    public string MinAmountB { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        var body = new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.LIQUIDITY_POOL_WITHDRAW),
            LiquidityPoolWithdrawOp = new LiquidityPoolWithdrawOp
            {
                LiquidityPoolID = LiquidityPoolID.ToXdr(),
                Amount = new xdr_Int64(ToXdrAmount(Amount)),
                MinAmountA = new xdr_Int64(ToXdrAmount(MinAmountA)),
                MinAmountB = new xdr_Int64(ToXdrAmount(MinAmountB))
            }
        };
        return body;
    }

    public class Builder
    {
        private readonly string _amount;
        private readonly LiquidityPoolID _liquidityPoolID;
        private readonly string _minAmountA;
        private readonly string _minAmountB;

        private KeyPair? _sourceAccount;

        public Builder(LiquidityPoolWithdrawOp operationXdr)
        {
            _liquidityPoolID = LiquidityPoolID.FromXdr(operationXdr.LiquidityPoolID);
            _amount = FromXdrAmount(operationXdr.Amount.InnerValue);
            _minAmountA = FromXdrAmount(operationXdr.MinAmountA.InnerValue);
            _minAmountB = FromXdrAmount(operationXdr.MinAmountB.InnerValue);
        }

        public Builder(LiquidityPoolID liquidityPoolID, string amount, string minAmountA, string minAmountB)
        {
            _liquidityPoolID = liquidityPoolID;
            _amount = amount;
            _minAmountA = minAmountA;
            _minAmountB = minAmountB;
        }

        public Builder(AssetAmount assetA, AssetAmount assetB, string amount)
        {
            _liquidityPoolID = new LiquidityPoolID(
                LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA.Asset,
                assetB.Asset,
                LiquidityPoolParameters.Fee);
            _amount = amount;
            _minAmountA = assetA.Amount;
            _minAmountB = assetB.Amount;
        }

        /// <summary>
        ///     Set source account of this operation
        /// </summary>
        /// <param name="sourceAccount">Source account</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetSourceAccount(KeyPair sourceAccount)
        {
            _sourceAccount = sourceAccount;
            return this;
        }

        public LiquidityPoolWithdrawOperation Build()
        {
            var operation = new LiquidityPoolWithdrawOperation(_liquidityPoolID, _amount, _minAmountA, _minAmountB);

            if (_sourceAccount != null) operation.SourceAccount = _sourceAccount;

            return operation;
        }
    }
}