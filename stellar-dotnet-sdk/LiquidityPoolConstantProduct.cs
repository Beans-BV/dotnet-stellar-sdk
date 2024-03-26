using System;
using stellar_dotnet_sdk.xdr;
using Int64 = stellar_dotnet_sdk.xdr.Int64;

namespace stellar_dotnet_sdk;

public class LiquidityPoolConstantProduct : LiquidityPoolEntryBody
{
    public LiquidityPoolConstantProduct(LiquidityPoolConstantProductParameters parameters, long reserveA, long reserveB,
        long totalPoolShares, long poolSharesTrustLineCount)
    {
        Parameters = parameters;
        ReserveA = reserveA;
        ReserveB = reserveB;
        TotalPoolShares = totalPoolShares;
        PoolSharesTrustLineCount = poolSharesTrustLineCount;
    }

    public LiquidityPoolConstantProductParameters Parameters { get; init; }
    public long ReserveA { get; init; }
    public long ReserveB { get; init; }
    public long TotalPoolShares { get; init; }
    public long PoolSharesTrustLineCount { get; init; }

    public LiquidityPoolEntry.LiquidityPoolEntryBody.LiquidityPoolEntryConstantProduct ToXdr()
    {
        return new LiquidityPoolEntry.LiquidityPoolEntryBody.LiquidityPoolEntryConstantProduct
        {
            Params = Parameters.ToXdr().ConstantProduct,
            ReserveA = new Int64(ReserveA),
            ReserveB = new Int64(ReserveB),
            TotalPoolShares = new Int64(TotalPoolShares),
            PoolSharesTrustLineCount = new Int64(PoolSharesTrustLineCount)
        };
    }

    public LiquidityPoolEntry.LiquidityPoolEntryBody ToXdrLiquidityPoolEntryBody()
    {
        return new LiquidityPoolEntry.LiquidityPoolEntryBody
        {
            Discriminant = new LiquidityPoolType
            {
                InnerValue = LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT
            },
            ConstantProduct = ToXdr()
        };
    }

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
            throw new ArgumentException("Not a LiquidityPoolConstantProduct", nameof(xdrBody));
        return FromXdr(xdrBody.ConstantProduct);
    }
}