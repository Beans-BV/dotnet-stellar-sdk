using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Xdr;
using Assets_Asset = StellarDotnetSdk.Assets.Asset;

namespace StellarDotnetSdk.LedgerKeys;

/// <summary>
///     Represents a ledger key for a liquidity pool entry on the Stellar network.
///     Used to look up liquidity pool data from the ledger by its pool ID.
/// </summary>
public class LedgerKeyLiquidityPool : LedgerKey
{
    /// <summary>
    ///     Constructs a <c>LedgerKeyLiquidityPool</c> from a liquidity pool ID.
    /// </summary>
    /// <param name="poolId">The unique identifier of the liquidity pool.</param>
    public LedgerKeyLiquidityPool(LiquidityPoolId poolId)
    {
        LiquidityPoolId = poolId;
    }

    /// <summary>
    ///     Constructs a <c>LedgerKeyLiquidityPool</c> from asset pair and fee, computing the pool ID automatically.
    /// </summary>
    /// <param name="assetA">The first asset in the pool (must be lexicographically less than assetB).</param>
    /// <param name="assetB">The second asset in the pool.</param>
    /// <param name="fee">The fee charged per trade in basis points (typically 30).</param>
    public LedgerKeyLiquidityPool(Assets_Asset assetA, Assets_Asset assetB, int fee)
    {
        LiquidityPoolId = new LiquidityPoolId(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
            assetA, assetB, fee);
    }

    /// <summary>
    ///     The unique identifier of the liquidity pool.
    /// </summary>
    public LiquidityPoolId LiquidityPoolId { get; }

    /// <summary>
    ///     Serializes this ledger key to its XDR representation.
    /// </summary>
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

    /// <summary>
    ///     Deserializes a <see cref="LedgerKeyLiquidityPool" /> from its XDR representation.
    /// </summary>
    /// <param name="xdr">The XDR ledger key liquidity pool object.</param>
    public static LedgerKeyLiquidityPool FromXdr(Xdr.LedgerKey.LedgerKeyLiquidityPool xdr)
    {
        return new LedgerKeyLiquidityPool(new LiquidityPoolId(xdr.LiquidityPoolID.InnerValue.InnerValue));
    }
}