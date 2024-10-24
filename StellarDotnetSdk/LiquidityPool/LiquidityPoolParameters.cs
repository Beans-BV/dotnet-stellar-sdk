using System;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;

namespace StellarDotnetSdk.LiquidityPool;

public abstract class LiquidityPoolParameters
{
    public const int Fee = 30;

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

    public abstract Xdr.LiquidityPoolParameters ToXdr();

    public abstract LiquidityPoolId GetId();
}