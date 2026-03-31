using System;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;

namespace StellarDotnetSdk.LiquidityPool;

/// <summary>
///     Base class for liquidity pool parameters on the Stellar network.
///     Defines the pool type, assets, and fee structure for an automated market maker (AMM) pool.
/// </summary>
public abstract class LiquidityPoolParameters
{
    /// <summary>
    ///     The default fee for liquidity pool trades in basis points (30 bp = 0.3%).
    /// </summary>
    public const int Fee = 30;

    /// <summary>
    ///     Creates a <see cref="LiquidityPoolParameters" /> instance for the specified pool type.
    /// </summary>
    /// <param name="type">The type of the liquidity pool.</param>
    /// <param name="assetA">The first asset in the pool (must be lexicographically less than assetB).</param>
    /// <param name="assetB">The second asset in the pool.</param>
    /// <param name="feeBp">The fee charged per trade in basis points.</param>
    public static LiquidityPoolParameters Create(
        LiquidityPoolType.LiquidityPoolTypeEnum type,
        Asset assetA,
        Asset assetB,
        int feeBp)
    {
        if (type != LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT)
        {
            throw new ArgumentException($"Unknown liquidity pool type {type}");
        }

        return new LiquidityPoolConstantProductParameters(assetA, assetB, feeBp);
    }

    /// <summary>
    ///     Deserializes a <see cref="LiquidityPoolParameters" /> from its XDR representation.
    /// </summary>
    /// <param name="liquidityPoolParametersXdr">The XDR liquidity pool parameters object.</param>
    public static LiquidityPoolParameters FromXdr(Xdr.LiquidityPoolParameters liquidityPoolParametersXdr)
    {
        switch (liquidityPoolParametersXdr.Discriminant.InnerValue)
        {
            case LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT:
                return LiquidityPoolConstantProductParameters.FromXdr(liquidityPoolParametersXdr.ConstantProduct);

            default:
                throw new ArgumentException(
                    $"Unknown liquidity pool type {liquidityPoolParametersXdr.Discriminant.InnerValue}");
        }
    }

    /// <summary>
    ///     Serializes this liquidity pool parameters object to its XDR representation.
    /// </summary>
    public abstract Xdr.LiquidityPoolParameters ToXdr();

    /// <summary>
    ///     Computes and returns the unique <see cref="LiquidityPoolId" /> for these parameters.
    /// </summary>
    public abstract LiquidityPoolId GetId();
}