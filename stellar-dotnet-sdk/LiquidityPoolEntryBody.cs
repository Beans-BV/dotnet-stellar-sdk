using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public abstract class LiquidityPoolEntryBody
{
    public LiquidityPoolEntry.LiquidityPoolEntryBody ToXdr()
    {
        return this switch
        {
            LiquidityPoolConstantProduct constantProduct => constantProduct.ToXdrLiquidityPoolEntryBody(),
            _ => throw new InvalidOperationException("Unknown liquidity pool type")
        };
    }

    public static LiquidityPoolEntryBody FromXdr(LiquidityPoolEntry.LiquidityPoolEntryBody xdrBody)
    {
        return xdrBody.Discriminant.InnerValue switch
        {
            LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT => LiquidityPoolConstantProduct
                .FromXdrLiquidityPoolEntryBody(xdrBody),
            _ => throw new InvalidOperationException("Unknown liquidity pool type")
        };
    }
}