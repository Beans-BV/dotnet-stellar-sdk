using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using ChangeTrustAsset = StellarDotnetSdk.Assets.ChangeTrustAsset;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using xdr_Operation = StellarDotnetSdk.Xdr.Operation;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Creates, updates, or deletes a trustline.
///     <p>Use <see cref="Builder" /> to create a new <c>ChangeTrustOperation</c>.</p>
///     <p>
///         See:
///         <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#change-trust">Change trust</a>
///     </p>
/// </summary>
public class ChangeTrustOperation : Operation
{
    public const string MaxLimit = "922337203685.4775807";

    private ChangeTrustOperation(ChangeTrustAsset asset, string limit)
    {
        Asset = asset ?? throw new ArgumentNullException(nameof(asset), "asset cannot be null");
        Limit = limit ?? throw new ArgumentNullException(nameof(limit), "limit cannot be null");
    }

    /// <summary>
    ///     The asset of the trustline.
    /// </summary>
    public ChangeTrustAsset Asset { get; }

    /// <summary>
    ///     The limit of the trustline.
    /// </summary>
    public string Limit { get; }

    public override xdr_Operation.OperationBody ToOperationBody()
    {
        return new xdr_Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CHANGE_TRUST),
            ChangeTrustOp = new ChangeTrustOp
            {
                Line = Asset.ToXdr(),
                Limit = new Int64(ToXdrAmount(Limit))
            }
        };
    }

    /// <summary>
    ///     Builder for <c>ChangeTrustOperation</c>.
    /// </summary>
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
        ///     Constructs a new <c>ChangeTrustOperation</c> builder.
        /// </summary>
        /// <param name="asset">
        ///     The asset of the trustline.
        /// </param>
        /// <param name="limit">
        ///     The limit of the trustline. For example, if a gateway extends a trustline of up to 200 USD to a
        ///     user, the limit is 200.
        ///     <p>Leave empty to default to the max int64.</p>
        ///     <p>Set to 0 to remove the trust line.</p>
        /// </param>
        [Obsolete("Deprecated. Use Builder(Asset asset, string? limit = null) instead.")]
        public Builder(ChangeTrustAsset asset, string? limit = null)
        {
            _asset = asset ?? throw new ArgumentNullException(nameof(asset), "asset cannot be null");
            _limit = limit ?? MaxLimit;
        }

        /// <summary>
        ///     Constructs a new <c>ChangeTrustOperation</c> builder.
        /// </summary>
        /// <param name="asset">
        ///     The asset of the trustline.
        /// </param>
        /// <param name="limit">
        ///     The limit of the trustline. For example, if a gateway extends a trustline of up to 200 USD to a
        ///     user, the limit is 200.
        ///     <p>Leave empty to default to the max int64.</p>
        ///     <p>Set to 0 to remove the trust line.</p>
        /// </param>
        public Builder(Asset asset, string? limit = null) : this(ChangeTrustAsset.Create(asset), limit)
        {
            _asset = asset == null
                ? throw new ArgumentNullException(nameof(asset), "asset cannot be null")
                : ChangeTrustAsset.Create(asset);
            _limit = limit ?? MaxLimit;
        }

        /// <summary>
        ///     Constructs a new <c>ChangeTrustOperation</c> builder for Liquidity pool shares type with the default fee.
        /// </summary>
        /// <param name="assetA">Asset A.</param>
        /// <param name="assetB">Asset B.</param>
        /// <param name="limit">
        ///     The limit of the trustline.
        ///     <p>Leave empty to default to the max int64.</p>
        ///     <p>Set to 0 to remove the trust line.</p>
        /// </param>
        public Builder(Asset assetA, Asset assetB, string? limit = null)
        {
            _asset = ChangeTrustAsset.Create(assetA, assetB, LiquidityPoolParameters.Fee);
            _limit = limit ?? MaxLimit;
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
        ///     Builds an operation.
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