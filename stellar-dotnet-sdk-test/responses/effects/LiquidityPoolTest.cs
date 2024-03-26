using Microsoft.VisualStudio.TestTools.UnitTesting;
using stellar_dotnet_sdk;
using stellar_dotnet_sdk.responses.effects;
using stellar_dotnet_sdk.xdr;
using Asset = stellar_dotnet_sdk.Asset;

namespace stellar_dotnet_sdk_test.responses.effects;

[TestClass]
public class LiquidityPoolTest
{
    [TestMethod]
    public void TestCreation()
    {
        var asset1 = Asset.CreateNonNativeAsset("TEST0", KeyPair.Random().AccountId);
        var asset2 = Asset.CreateNonNativeAsset("TEST1", KeyPair.Random().AccountId);

        var liquidityPoolID =
            new LiquidityPoolID(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, asset1, asset2,
                100);
        var reserve = new AssetAmount(asset1, "100");

        var liquidityPool = new LiquidityPool(liquidityPoolID, 100,
            LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, 1, "50", new[] { reserve });

        Assert.AreEqual(liquidityPool.ID, liquidityPoolID);
        Assert.AreEqual(liquidityPool.FeeBP, 100);
        Assert.AreEqual(liquidityPool.Type, LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT);
        Assert.AreEqual(liquidityPool.TotalTrustlines, 1);
        Assert.AreEqual(liquidityPool.TotalShares, "50");
        Assert.AreEqual(liquidityPool.Reserves[0], reserve);
    }
}