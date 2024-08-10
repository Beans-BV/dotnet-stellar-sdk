using System;
using StellarDotnetSdk.Xdr;
using LiquidityPoolConstantProductParameters = StellarDotnetSdk.LiquidityPool.LiquidityPoolConstantProductParameters;

namespace StellarDotnetSdk.LedgerEntries;

public class LiquidityPoolConstantProduct : LiquidityPoolEntryBody
{
    private LiquidityPoolConstantProduct(LiquidityPoolConstantProductParameters parameters, long reserveA,
        long reserveB,
        long totalPoolShares, long poolSharesTrustLineCount)
    {
        Parameters = parameters;
        ReserveA = reserveA;
        ReserveB = reserveB;
        TotalPoolShares = totalPoolShares;
        PoolSharesTrustLineCount = poolSharesTrustLineCount;
    }

    public LiquidityPoolConstantProductParameters Parameters { get; }
    public long ReserveA { get; }
    public long ReserveB { get; }
    public long TotalPoolShares { get; }
    public long PoolSharesTrustLineCount { get; }

    public static LiquidityPoolConstantProduct FromXdr(
        LiquidityPoolEntry.LiquidityPoolEntryBody.LiquidityPoolEntryConstantProduct xdrConstantProduct)
    {
        var parameters = LiquidityPoolConstantProductParameters.FromXdr(xdrConstantProduct.Params);
        var reserveA = xdrConstantProduct.ReserveA.InnerValue;
        var reserveB = xdrConstantProduct.ReserveB.InnerValue;
        var totalPoolShares = xdrConstantProduct.TotalPoolShares.InnerValue;
        var poolSharesTrustLineCount = xdrConstantProduct.PoolSharesTrustLineCount.InnerValue;
        return new LiquidityPoolConstantProduct(parameters, reserveA, reserveB, totalPoolShares,
            poolSharesTrustLineCount);
    }

    public static LiquidityPoolConstantProduct FromXdrLiquidityPoolEntryBody(
        LiquidityPoolEntry.LiquidityPoolEntryBody xdrBody)
    {
        if (xdrBody.Discriminant.InnerValue != LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT)
        {
            throw new ArgumentException("Not a LiquidityPoolConstantProduct", nameof(xdrBody));
        }
        return FromXdr(xdrBody.ConstantProduct);
    }
}