using System;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using Int32 = StellarDotnetSdk.Xdr.Int32;

namespace StellarDotnetSdk.LiquidityPool;

using Asset = Asset;
using Int32 = Int32;

/// <summary>
///     Represents the parameters for a constant-product liquidity pool (x * y = k).
///     The two assets must be in lexicographic order (Asset A &lt; Asset B).
/// </summary>
public class LiquidityPoolConstantProductParameters : LiquidityPoolParameters
{
    /// <summary>
    ///     Constructs a <c>LiquidityPoolConstantProductParameters</c> with the two assets and fee.
    ///     Assets must be in lexicographic order (Asset A &lt; Asset B).
    /// </summary>
    /// <param name="assetA">The first asset in the pool (must be lexicographically less than assetB).</param>
    /// <param name="assetB">The second asset in the pool.</param>
    /// <param name="feeBp">The fee charged per trade in basis points (typically 30).</param>
    public LiquidityPoolConstantProductParameters(Asset assetA, Asset assetB, int feeBp)
    {
        AssetA = assetA ?? throw new ArgumentNullException(nameof(assetA), "assetA cannot be null");
        AssetB = assetB ?? throw new ArgumentNullException(nameof(assetB), "assetB cannot be null");

        if (assetA.CompareTo(assetB) >= 0)
        {
            throw new ArgumentException("Asset A must be < Asset B (Lexicographic Order).");
        }

        Fee = feeBp;
    }

    /// <summary>
    ///     The first asset in the pool (lexicographically smaller).
    /// </summary>
    public Asset AssetA { get; }

    /// <summary>
    ///     The second asset in the pool (lexicographically larger).
    /// </summary>
    public Asset AssetB { get; }

    /// <summary>
    ///     The fee charged per trade in basis points (1 bp = 0.01%).
    /// </summary>
    public new int Fee { get; }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is not LiquidityPoolConstantProductParameters other)
        {
            return false;
        }
        return Equals(AssetA, other.AssetA) && Equals(AssetB, other.AssetB) && Equals(Fee, other.Fee);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return AssetA.GetHashCode() ^ AssetB.GetHashCode() ^ Fee;
    }

    /// <summary>
    ///     Serializes this constant product parameters object to its XDR representation.
    /// </summary>
    public override Xdr.LiquidityPoolParameters ToXdr()
    {
        var liquidityPoolParametersXdr = new Xdr.LiquidityPoolParameters
        {
            Discriminant =
            {
                InnerValue = LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
            },
        };

        var parameters = new Xdr.LiquidityPoolConstantProductParameters
        {
            AssetA = AssetA.ToXdr(),
            AssetB = AssetB.ToXdr(),
            Fee = new Int32(Fee),
        };

        liquidityPoolParametersXdr.ConstantProduct = parameters;

        return liquidityPoolParametersXdr;
    }

    /// <summary>
    ///     Deserializes a <see cref="LiquidityPoolConstantProductParameters" /> from its XDR representation.
    /// </summary>
    /// <param name="liquidityPoolConstantProductParametersXdr">The XDR constant product parameters object.</param>
    public static LiquidityPoolConstantProductParameters FromXdr(
        Xdr.LiquidityPoolConstantProductParameters liquidityPoolConstantProductParametersXdr)
    {
        return new LiquidityPoolConstantProductParameters(
            Asset.FromXdr(liquidityPoolConstantProductParametersXdr.AssetA),
            Asset.FromXdr(liquidityPoolConstantProductParametersXdr.AssetB),
            liquidityPoolConstantProductParametersXdr.Fee.InnerValue);
    }

    /// <summary>
    ///     Computes and returns the liquidity pool ID for these parameters.
    /// </summary>
    public override LiquidityPoolId GetId()
    {
        return new LiquidityPoolId(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, AssetA,
            AssetB, Fee);
    }
}