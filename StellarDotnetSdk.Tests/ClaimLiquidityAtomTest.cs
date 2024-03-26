using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
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

        var liquidityPool = new LiquidityPoolID(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
            asset1, asset2, 100);
        claimLiquidityAtomXdr.LiquidityPoolID = liquidityPool.ToXdr();

        var claimLiquidityAtom = StellarDotnetSdk.Responses.Results.ClaimLiquidityAtom.FromXdr(claimLiquidityAtomXdr);

        Assert.AreEqual(claimLiquidityAtom.AmountBought, "0.00001");
        Assert.AreEqual(claimLiquidityAtom.AmountSold, "0.00001");
        Assert.AreEqual(claimLiquidityAtom.AssetBought, asset1);
        Assert.AreEqual(claimLiquidityAtom.AssetSold, asset2);
        Assert.AreEqual(claimLiquidityAtom.LiquidityPoolID.Hash, liquidityPool.Hash);
    }
}