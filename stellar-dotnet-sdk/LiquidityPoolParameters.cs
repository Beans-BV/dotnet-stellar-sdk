﻿using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public abstract class LiquidityPoolParameters
{
    public const int Fee = 30;

    public static LiquidityPoolParameters Create(LiquidityPoolType.LiquidityPoolTypeEnum type, Asset assetA,
        Asset assetB, int feeBP)
    {
        if (type != LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT)
            throw new ArgumentException($"Unknown liquidity pool type {type}");

        return new LiquidityPoolConstantProductParameters(assetA, assetB, feeBP);
    }

    public static LiquidityPoolParameters FromXdr(xdr.LiquidityPoolParameters liquidityPoolParametersXdr)
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

    public abstract xdr.LiquidityPoolParameters ToXdr();

    public abstract LiquidityPoolID GetID();
}