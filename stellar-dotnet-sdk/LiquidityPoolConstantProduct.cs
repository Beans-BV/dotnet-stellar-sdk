using System;
using stellar_dotnet_sdk.xdr;
using Int64 = stellar_dotnet_sdk.xdr.Int64;

namespace stellar_dotnet_sdk;

public class LiquidityPoolConstantProduct : LiquidityPoolEntryBody
{
    public LiquidityPoolConstantProductParameters Parameters { get; set; }
    public long ReserveA { get; set; }
    public long ReserveB { get; set; }
    public long TotalPoolShares { get; set; }
    public long PoolSharesTrustLineCount { get; set; }

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

    public static LiquidityPoolConstantProduct FromXdr(LiquidityPoolEntry.LiquidityPoolEntryBody.LiquidityPoolEntryConstantProduct xdrConstantProduct)
    {
        return new LiquidityPoolConstantProduct
        {
            Parameters = LiquidityPoolConstantProductParameters.FromXdr(xdrConstantProduct.Params),
            ReserveA = xdrConstantProduct.ReserveA.InnerValue,
            ReserveB = xdrConstantProduct.ReserveB.InnerValue,
            TotalPoolShares = xdrConstantProduct.TotalPoolShares.InnerValue,
            PoolSharesTrustLineCount = xdrConstantProduct.PoolSharesTrustLineCount.InnerValue,
        };
    }
    public static LiquidityPoolConstantProduct FromXdrLiquidityPoolEntryBody(
        LiquidityPoolEntry.LiquidityPoolEntryBody xdrBody)
    {
        if (xdrBody.Discriminant.InnerValue != LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT)
            throw new ArgumentException("Not a LiquidityPoolConstantProduct", nameof(xdrBody));
        return FromXdr(xdrBody.ConstantProduct);
    }
}