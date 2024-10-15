using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Xdr;
using Assets_Asset = StellarDotnetSdk.Assets.Asset;

namespace StellarDotnetSdk.LedgerKeys;

public class LedgerKeyLiquidityPool : LedgerKey
{
    public LedgerKeyLiquidityPool(LiquidityPoolId poolId)
    {
        LiquidityPoolId = poolId;
    }

    public LedgerKeyLiquidityPool(Assets_Asset assetA, Assets_Asset assetB, int fee)
    {
        LiquidityPoolId = new LiquidityPoolId(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
            assetA, assetB, fee);
    }

    public LiquidityPoolId LiquidityPoolId { get; }

    public override Xdr.LedgerKey ToXdr()
    {
        return new Xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.LIQUIDITY_POOL },
            LiquidityPool = new Xdr.LedgerKey.LedgerKeyLiquidityPool
            {
                LiquidityPoolID = new PoolID(new Hash(LiquidityPoolId.Hash)),
            },
        };
    }

    public static LedgerKeyLiquidityPool FromXdr(Xdr.LedgerKey.LedgerKeyLiquidityPool xdr)
    {
        return new LedgerKeyLiquidityPool(new LiquidityPoolId(xdr.LiquidityPoolID.InnerValue.InnerValue));
    }
}