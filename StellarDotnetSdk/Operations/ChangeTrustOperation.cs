using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using ChangeTrustAsset = StellarDotnetSdk.Assets.ChangeTrustAsset;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using LiquidityPoolConstantProductParameters = StellarDotnetSdk.LiquidityPool.LiquidityPoolConstantProductParameters;
using LiquidityPoolParameters = StellarDotnetSdk.LiquidityPool.LiquidityPoolParameters;
using xdr_Operation = StellarDotnetSdk.Xdr.Operation;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Creates, updates, or deletes a trustline.
///     <p>
///         See:
///         <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#change-trust">Change trust</a>
///     </p>
/// </summary>
public class ChangeTrustOperation : Operation
{
    public const string MaxLimit = "922337203685.4775807";

    /// <summary>
    ///     Constructs a new <c>ChangeTrustOperation</c>.
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
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public ChangeTrustOperation(Asset asset, string? limit = null, IAccountId? sourceAccount = null) : base(
        sourceAccount)
    {
        Asset = asset == null
            ? throw new ArgumentNullException(nameof(asset), "asset cannot be null")
            : ChangeTrustAsset.Create(asset);
        Limit = limit ?? MaxLimit;
    }

    /// <summary>
    ///     Constructs a new <c>ChangeTrustOperation</c> for Liquidity pool shares type with the default fee.
    /// </summary>
    /// <param name="assetA">Asset A.</param>
    /// <param name="assetB">Asset B.</param>
    /// <param name="feeBP">The fee in base points.</param>
    /// <param name="limit">
    ///     The limit of the trustline.
    ///     <p>Leave empty to default to the max int64.</p>
    ///     <p>Set to 0 to remove the trust line.</p>
    /// </param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    [Obsolete("Use the constructor with the ChangeTrustAsset parameter instead.")]
    public ChangeTrustOperation(Asset assetA, Asset assetB, int? feeBP = null, string? limit = null, IAccountId? sourceAccount = null) :
        base(sourceAccount)
    {
        Asset = ChangeTrustAsset.Create(assetA, assetB, feeBP ?? LiquidityPoolParameters.Fee);
        Limit = limit ?? MaxLimit;
    }

    /// <summary>
    ///   Constructs a new <c>ChangeTrustOperation</c> for Liquidity pool shares type with the default fee.
    /// </summary>
    /// <param name="changeTrustAsset">The Change Trust Asset</param>
    /// <param name="limit">
    ///     The limit of the trustline.
    ///     <p>Leave empty to default to the max int64.</p>
    ///     <p>Set to 0 to remove the trust line.</p>
    /// </param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public ChangeTrustOperation(ChangeTrustAsset changeTrustAsset, string? limit = null, IAccountId? sourceAccount = null) :
        base(sourceAccount)
    {
        Asset = changeTrustAsset;
        Limit = limit ?? MaxLimit;
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
                Limit = new Int64(ToXdrAmount(Limit)),
            },
        };
    }

    public static ChangeTrustOperation FromXdr(ChangeTrustOp changeTrustOp)
    {
        switch (changeTrustOp.Line.Discriminant.InnerValue)
        {
            case AssetType.AssetTypeEnum.ASSET_TYPE_NATIVE:
            case AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM4:
            case AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM12:
                var wrapper = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.FromXdr(changeTrustOp.Line);
                return new ChangeTrustOperation(wrapper.Asset, Amount.FromXdr(changeTrustOp.Limit.InnerValue));
            case AssetType.AssetTypeEnum.ASSET_TYPE_POOL_SHARE:
                var liquidityPoolShareChangeTrustAsset =
                    (LiquidityPoolShareChangeTrustAsset)ChangeTrustAsset.FromXdr(changeTrustOp.Line);
                var parameters =
                    (LiquidityPoolConstantProductParameters)liquidityPoolShareChangeTrustAsset.Parameters;
                return new ChangeTrustOperation(
                    parameters.AssetA, parameters.AssetB, parameters.Fee, Amount.FromXdr(changeTrustOp.Limit.InnerValue)
                );
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}