using System;
using stellar_dotnet_sdk.xdr;
using sdkxdr = stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
///     Represents a <see cref="ChangeTrustOp" />.
///     Use <see cref="Builder" /> to create a new ChangeTrustOperation.
///     See also:
///     <see href="https://www.stellar.org/developers/guides/concepts/list-of-operations.html#change-trust">Change Trust</see>
/// </summary>
public class ChangeTrustOperation : Operation
{
    public const string MaxLimit = "922337203685.4775807";

    private ChangeTrustOperation(ChangeTrustAsset asset, string limit)
    {
        Asset = asset ?? throw new ArgumentNullException(nameof(asset), "asset cannot be null");
        Limit = limit ?? throw new ArgumentNullException(nameof(limit), "limit cannot be null");
    }

    public ChangeTrustAsset Asset { get; }
    public string Limit { get; }

    public override sdkxdr.Operation.OperationBody ToOperationBody()
    {
        var op = new ChangeTrustOp
        {
            Line = Asset.ToXdr()
        };
        var limit = new sdkxdr.Int64
        {
            InnerValue = ToXdrAmount(Limit)
        };
        op.Limit = limit;

        var body = new sdkxdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CHANGE_TRUST),
            ChangeTrustOp = op
        };
        return body;
    }

    /// <summary>
    ///     Builds ChangeTrust operation.
    /// </summary>
    /// <see cref="ChangeTrustOperation" />
    public class Builder
    {
        private readonly ChangeTrustAsset _asset;
        private readonly string _limit;

        private KeyPair? _mSourceAccount;

        public Builder(ChangeTrustOp op)
        {
            _asset = ChangeTrustAsset.FromXdr(op.Line);
            _limit = FromXdrAmount(op.Limit.InnerValue);
        }

        /// <summary>
        ///     Creates a new ChangeTrust builder.
        /// </summary>
        /// <param name="asset">
        ///     The asset of the trustline. For example, if a gateway extends a trustline of up to 200 USD to a
        ///     user, the line is USD.
        /// </param>
        /// <param name="limit">
        ///     The limit of the trustline. For example, if a gateway extends a trustline of up to 200 USD to a
        ///     user, the limit is 200.
        ///     <p>Leave empty to default to the max int64.</p>
        ///     <p>Set to 0 to remove the trust line.</p>
        /// </param>
        /// <exception cref="ArithmeticException">When limit has more than 7 decimal places.</exception>
        public Builder(ChangeTrustAsset asset, string? limit = null)
        {
            _asset = asset ?? throw new ArgumentNullException(nameof(asset), "asset cannot be null");
            _limit = limit ?? MaxLimit;
        }

        public Builder(Asset asset, string? limit = null) : this(ChangeTrustAsset.Create(asset), limit)
        {
        }
        
        /// <summary>
        /// Creates a new ChangeTrust builder for Liquidity pool shares type.
        /// </summary>
        /// <param name="assetA"></param>
        /// <param name="assetB"></param>
        /// <param name="feeBP">For now the only fee supported is 30.</param>
        /// <param name="limit">
        ///     <p>(Optional) Leave empty to default to the max int64.</p>
        ///     <p>Set to 0 to remove the trust line.</p>
        /// </param>
        public Builder(Asset assetA, Asset assetB, int feeBP, string? limit = null)
        {
            if (feeBP != 30)
                throw new ArgumentException("Invalid fee.", nameof(feeBP));
            _asset = ChangeTrustAsset.Create(assetA, assetB, feeBP);
            _limit = limit ?? MaxLimit;
        }

        /// <summary>
        /// Creates a new ChangeTrust builder for Liquidity pool shares type with the default fee.
        /// </summary>
        /// <param name="assetA"></param>
        /// <param name="assetB"></param>
        /// <param name="feeBP">For now the only fee supported is 30.</param>
        /// <param name="limit">
        ///     <p>(Optional) Leave empty to default to the max int64.</p>
        ///     <p>Set to 0 to remove the trust line.</p>
        /// </param>
        public Builder(Asset assetA, Asset assetB, string? limit = null) : this(assetA, assetB, 30, limit)
        {
        }
        
        /// <summary>
        ///     Set source account of this operation
        /// </summary>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetSourceAccount(KeyPair sourceAccount)
        {
            _mSourceAccount = sourceAccount ??
                              throw new ArgumentNullException(nameof(sourceAccount), "sourceAccount cannot be null");
            return this;
        }

        /// <summary>
        ///     Builds an operation
        /// </summary>
        public ChangeTrustOperation Build()
        {
            var operation = new ChangeTrustOperation(_asset, _limit);
            if (_mSourceAccount != null)
                operation.SourceAccount = _mSourceAccount;
            return operation;
        }
    }
}