using System;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using Int32 = StellarDotnetSdk.Xdr.Int32;

namespace StellarDotnetSdk.LiquidityPool;

using Asset = Asset;
using Int32 = Int32;

public class LiquidityPoolConstantProductParameters : LiquidityPoolParameters
{
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

    public Asset AssetA { get; }
    public Asset AssetB { get; }
    public new int Fee { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not LiquidityPoolConstantProductParameters other)
        {
            return false;
        }
        return Equals(AssetA, other.AssetA) && Equals(AssetB, other.AssetB) && Equals(Fee, other.Fee);
    }

    public override int GetHashCode()
    {
        return AssetA.GetHashCode() ^ AssetB.GetHashCode() ^ Fee;
    }

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

    public static LiquidityPoolConstantProductParameters FromXdr(
        Xdr.LiquidityPoolConstantProductParameters liquidityPoolConstantProductParametersXdr)
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