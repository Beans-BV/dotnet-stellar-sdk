using System;
using stellar_dotnet_sdk.xdr;
using Int32 = stellar_dotnet_sdk.xdr.Int32;

namespace stellar_dotnet_sdk;

public class LiquidityPoolConstantProductParameters : LiquidityPoolParameters
{
    public LiquidityPoolConstantProductParameters(Asset assetA, Asset assetB, int feeBP)
    {
        AssetA = assetA ?? throw new ArgumentNullException(nameof(assetA), "assetA cannot be null");
        AssetB = assetB ?? throw new ArgumentNullException(nameof(assetB), "assetB cannot be null");
        Fee = feeBP;
    }

    public Asset AssetA { get; }
    public Asset AssetB { get; }
    public new int Fee { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not LiquidityPoolConstantProductParameters other) return false;
        return Equals(AssetA, other.AssetA) && Equals(AssetB, other.AssetB) && Equals(Fee, other.Fee);
    }

    public override int GetHashCode()
    {
        return AssetA.GetHashCode() ^ AssetB.GetHashCode() ^ Fee;
    }

    public override xdr.LiquidityPoolParameters ToXdr()
    {
        var liquidityPoolParametersXdr = new xdr.LiquidityPoolParameters
        {
            Discriminant =
            {
                InnerValue = LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT
            }
        };

        var parameters = new xdr.LiquidityPoolConstantProductParameters
        {
            AssetA = AssetA.ToXdr(),
            AssetB = AssetB.ToXdr(),
            Fee = new Int32(Fee)
        };

        liquidityPoolParametersXdr.ConstantProduct = parameters;

        return liquidityPoolParametersXdr;
    }

    public static LiquidityPoolConstantProductParameters FromXdr(
        xdr.LiquidityPoolConstantProductParameters liquidityPoolConstantProductParametersXdr)
    {
        return new LiquidityPoolConstantProductParameters(
            Asset.FromXdr(liquidityPoolConstantProductParametersXdr.AssetA),
            Asset.FromXdr(liquidityPoolConstantProductParametersXdr.AssetB),
            liquidityPoolConstantProductParametersXdr.Fee.InnerValue);
    }

    public override LiquidityPoolID GetID()
    {
        return new LiquidityPoolID(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, AssetA,
            AssetB, Fee);
    }
}