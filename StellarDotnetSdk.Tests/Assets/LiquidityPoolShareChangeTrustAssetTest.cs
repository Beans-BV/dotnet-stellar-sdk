using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using XDR = StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using LiquidityPoolParameters = StellarDotnetSdk.LiquidityPool.LiquidityPoolParameters;

namespace StellarDotnetSdk.Tests.Assets;

[TestClass]
public class LiquidityPoolShareChangeTrustAssetTest
{
    /// <summary>
    ///     Tests that LiquidityPoolShareChangeTrustAsset constructor throws ArgumentNullException when parameters are null.
    ///     Validates that liquidity pool parameters are required for creating pool share change trust assets.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestConstructor_NullParameters()
    {
        _ = new LiquidityPoolShareChangeTrustAsset(null!);
    }

    /// <summary>
    ///     Tests LiquidityPoolShareChangeTrustAsset creation using the constructor with two assets and fee.
    ///     Verifies that a liquidity pool share change trust asset is created with the correct pool parameters and type.
    /// </summary>
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

    /// <summary>
    ///     Tests GetLiquidityPoolId method to retrieve the unique pool identifier.
    ///     Verifies that the pool ID matches the ID from the underlying liquidity pool parameters.
    /// </summary>
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

    /// <summary>
    ///     Tests ToString method returns the string representation of the liquidity pool ID.
    ///     Verifies that ToString delegates to the pool ID's string representation.
    /// </summary>
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

    /// <summary>
    ///     Tests that LiquidityPoolShareChangeTrustAsset instances with the same parameters are considered equal.
    ///     Verifies that Equals implementation correctly compares the underlying liquidity pool parameters.
    /// </summary>
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

    /// <summary>
    ///     Tests that LiquidityPoolShareChangeTrustAsset instances with different parameters are not equal.
    ///     Verifies that Equals correctly distinguishes between different liquidity pools (different assets or fees).
    /// </summary>
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

    /// <summary>
    ///     Tests that LiquidityPoolShareChangeTrustAsset.Equals returns false when comparing with null.
    ///     Verifies null safety in the Equals implementation.
    /// </summary>
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

        Assert.IsFalse(liquidityPoolShareChangeTrustAsset.Equals(null!));
    }

    /// <summary>
    ///     Tests that LiquidityPoolShareChangeTrustAsset instances with the same parameters produce the same hash code.
    ///     Verifies GetHashCode implementation for proper behavior in hash-based collections.
    /// </summary>
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

    /// <summary>
    ///     Tests comparison ordering between LiquidityPoolShareChangeTrustAsset and ChangeTrustAsset.Wrapper.
    ///     Verifies that pool share assets sort after regular asset wrappers (returns 1).
    /// </summary>
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

    /// <summary>
    ///     Tests comparison ordering between two LiquidityPoolShareChangeTrustAsset instances.
    ///     Verifies that CompareTo correctly compares different liquidity pools based on their parameters.
    /// </summary>
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

    /// <summary>
    ///     Tests XDR serialization and deserialization round-trip for LiquidityPoolShareChangeTrustAsset.
    ///     Verifies that liquidity pool share change trust assets can be converted to XDR format and back,
    ///     preserving the pool parameters and ID.
    /// </summary>
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