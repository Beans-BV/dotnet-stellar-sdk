using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;

namespace StellarDotnetSdk.Tests;

[TestClass]
public class ClaimLiquidityAtomTest
{
    [TestMethod]
    public void TestFromXdr()
    {
        var claimLiquidityAtomXdr = new ClaimLiquidityAtom();

        var asset1 = Asset.CreateNonNativeAsset("TEST0", KeyPair.Random().AccountId);
        var asset2 = Asset.CreateNonNativeAsset("TEST1", KeyPair.Random().AccountId);

        claimLiquidityAtomXdr.AmountBought = new Int64(100L);
        claimLiquidityAtomXdr.AmountSold = new Int64(100L);
        claimLiquidityAtomXdr.AssetBought = asset1.ToXdr();
        claimLiquidityAtomXdr.AssetSold = asset2.ToXdr();

        var liquidityPool = new LiquidityPoolId(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
            asset1, asset2, 100);
        claimLiquidityAtomXdr.LiquidityPoolID = liquidityPool.ToXdr();
        var claimAtom = new StellarDotnetSdk.Xdr.ClaimAtom
        {
            Discriminant = new ClaimAtomType
            {
                InnerValue = ClaimAtomType.ClaimAtomTypeEnum.CLAIM_ATOM_TYPE_LIQUIDITY_POOL,
            },
            LiquidityPool = claimLiquidityAtomXdr,
        };

        var claimAtomLiquidityPool = (ClaimAtomLiquidityPool)ClaimAtom.FromXdr(claimAtom);

        Assert.AreEqual(claimAtomLiquidityPool.AmountBought, "0.00001");
        Assert.AreEqual(claimAtomLiquidityPool.AmountSold, "0.00001");
        Assert.AreEqual(claimAtomLiquidityPool.AssetBought, asset1);
        Assert.AreEqual(claimAtomLiquidityPool.AssetSold, asset2);
        Assert.AreEqual(claimAtomLiquidityPool.LiquidityPoolId.Hash, liquidityPool.Hash);
    }
}