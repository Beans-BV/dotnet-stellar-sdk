using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerKeyLiquidityPool : LedgerKey
{
    public LedgerKeyLiquidityPool(LiquidityPoolID poolId)
    {
        LiquidityPoolID = poolId;
    }

    public LedgerKeyLiquidityPool(Asset assetA, Asset assetB, int fee)
    {
        LiquidityPoolID = new LiquidityPoolID(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
            assetA, assetB, fee);
    }
    
    public LiquidityPoolID LiquidityPoolID { get; }

    public override xdr.LedgerKey ToXdr()
    {
        return new xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.LIQUIDITY_POOL },
            LiquidityPool = new xdr.LedgerKey.LedgerKeyLiquidityPool
            {
                LiquidityPoolID = new PoolID(new xdr.Hash(LiquidityPoolID.Hash))
            }
        };
    }

    public static LedgerKeyLiquidityPool FromXdr(xdr.LedgerKey.LedgerKeyLiquidityPool xdr)
    {
        return new LedgerKeyLiquidityPool(new LiquidityPoolID(xdr.LiquidityPoolID.InnerValue.InnerValue));
    }
}