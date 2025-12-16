using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.LiquidityPool;
using XDR = StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using LiquidityPoolParameters = StellarDotnetSdk.LiquidityPool.LiquidityPoolParameters;

namespace StellarDotnetSdk.Tests.Assets;

[TestClass]
public class LiquidityPoolShareChangeTrustAssetTest
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestConstructor_NullParameters()
    {
        new LiquidityPoolShareChangeTrustAsset((LiquidityPoolParameters)null!);
    }

    [TestMethod]
    public void TestConstructor_WithAssetAAndAssetB()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var liquidityPoolShareChangeTrustAsset = new LiquidityPoolShareChangeTrustAsset(assetA, assetB, 30);
        Assert.IsNotNull(liquidityPoolShareChangeTrustAsset.Parameters);
        Assert.AreEqual("pool_share", liquidityPoolShareChangeTrustAsset.Type);
    }

    [TestMethod]
    public void TestGetLiquidityPoolId()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var parameters =
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30);
        var liquidityPoolShareChangeTrustAsset = new LiquidityPoolShareChangeTrustAsset(parameters);

        var poolId = liquidityPoolShareChangeTrustAsset.GetLiquidityPoolId();
        Assert.IsNotNull(poolId);
        Assert.AreEqual(parameters.GetId(), poolId);
    }

    [TestMethod]
    public void TestToString()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var parameters =
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30);
        var liquidityPoolShareChangeTrustAsset = new LiquidityPoolShareChangeTrustAsset(parameters);

        var poolId = liquidityPoolShareChangeTrustAsset.GetLiquidityPoolId();
        var toStringResult = liquidityPoolShareChangeTrustAsset.ToString();
        Assert.AreEqual(poolId.ToString(), toStringResult);
    }

    [TestMethod]
    public void TestEquals_SameParameters()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var parameters =
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30);
        var liquidityPoolShareChangeTrustAsset1 = new LiquidityPoolShareChangeTrustAsset(parameters);
        var liquidityPoolShareChangeTrustAsset2 = new LiquidityPoolShareChangeTrustAsset(parameters);

        Assert.IsTrue(liquidityPoolShareChangeTrustAsset1.Equals(liquidityPoolShareChangeTrustAsset2));
    }

    [TestMethod]
    public void TestEquals_DifferentParameters()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();
        var keypair3 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");
        var assetC = Asset.Create($"GBP:{keypair3.AccountId}");

        var parameters1 =
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30);
        var parameters2 =
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetC, 30);

        var liquidityPoolShareChangeTrustAsset1 = new LiquidityPoolShareChangeTrustAsset(parameters1);
        var liquidityPoolShareChangeTrustAsset2 = new LiquidityPoolShareChangeTrustAsset(parameters2);

        Assert.IsFalse(liquidityPoolShareChangeTrustAsset1.Equals(liquidityPoolShareChangeTrustAsset2));
    }

    [TestMethod]
    public void TestEquals_Null()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var parameters =
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30);
        var liquidityPoolShareChangeTrustAsset = new LiquidityPoolShareChangeTrustAsset(parameters);

        Assert.IsFalse(liquidityPoolShareChangeTrustAsset.Equals(null));
    }

    [TestMethod]
    public void TestGetHashCode_Consistency()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var parameters =
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30);
        var liquidityPoolShareChangeTrustAsset1 = new LiquidityPoolShareChangeTrustAsset(parameters);
        var liquidityPoolShareChangeTrustAsset2 = new LiquidityPoolShareChangeTrustAsset(parameters);

        Assert.AreEqual(liquidityPoolShareChangeTrustAsset1.GetHashCode(),
            liquidityPoolShareChangeTrustAsset2.GetHashCode());
    }

    [TestMethod]
    public void TestCompareTo_WithWrapper()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var parameters =
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30);
        var liquidityPoolShareChangeTrustAsset = new LiquidityPoolShareChangeTrustAsset(parameters);

        var wrapper = new ChangeTrustAsset.Wrapper(Asset.Create($"USD:{keypair.AccountId}"));

        var comparison = liquidityPoolShareChangeTrustAsset.CompareTo(wrapper);
        Assert.AreEqual(1, comparison);
    }

    [TestMethod]
    public void TestCompareTo_WithPoolShare()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();
        var keypair3 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");
        var assetC = Asset.Create($"GBP:{keypair3.AccountId}");

        var parameters1 =
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30);
        var parameters2 =
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetC, 30);

        var liquidityPoolShareChangeTrustAsset1 = new LiquidityPoolShareChangeTrustAsset(parameters1);
        var liquidityPoolShareChangeTrustAsset2 = new LiquidityPoolShareChangeTrustAsset(parameters2);

        var comparison = liquidityPoolShareChangeTrustAsset1.CompareTo(liquidityPoolShareChangeTrustAsset2);
        Assert.IsTrue(comparison != 0);
    }

    [TestMethod]
    public void TestToXdr_RoundTrip()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var parameters =
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30);
        var liquidityPoolShareChangeTrustAsset = new LiquidityPoolShareChangeTrustAsset(parameters);

        var xdr = liquidityPoolShareChangeTrustAsset.ToXdr();
        var restored = ChangeTrustAsset.FromXdr(xdr);

        Assert.IsInstanceOfType(restored, typeof(LiquidityPoolShareChangeTrustAsset));
        var restoredPoolShare = (LiquidityPoolShareChangeTrustAsset)restored;
        Assert.AreEqual(liquidityPoolShareChangeTrustAsset.Parameters.GetId(),
            restoredPoolShare.Parameters.GetId());
    }
}


