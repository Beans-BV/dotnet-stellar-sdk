using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

public abstract class LiquidityPoolEntryBody
{
    public static LiquidityPoolEntryBody FromXdr(LiquidityPoolEntry.LiquidityPoolEntryBody xdrBody)
    {
        return xdrBody.Discriminant.InnerValue switch
        {
            LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT => LiquidityPoolConstantProduct
                .FromXdrLiquidityPoolEntryBody(xdrBody),
            _ => throw new InvalidOperationException("Unknown liquidity pool type"),
        };
    }
}