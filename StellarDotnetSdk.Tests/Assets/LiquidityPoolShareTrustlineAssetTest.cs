using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using LiquidityPoolParameters = StellarDotnetSdk.LiquidityPool.LiquidityPoolParameters;

namespace StellarDotnetSdk.Tests.Assets;

[TestClass]
public class LiquidityPoolShareTrustlineAssetTest
{
    /// <summary>
    ///     Tests that LiquidityPoolShareTrustlineAsset instances with the same pool ID are considered equal.
    ///     Verifies that Equals implementation correctly compares the underlying LiquidityPoolId instances.
    /// </summary>
    [TestMethod]
    public void TestEquality()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var trustlineAsset = (LiquidityPoolShareTrustlineAsset)TrustlineAsset.Create(
            LiquidityPoolParameters.Create(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30));
        var trustlineAsset2 = new LiquidityPoolShareTrustlineAsset(trustlineAsset.Id);
        Assert.IsTrue(trustlineAsset.Equals(trustlineAsset2));
    }

    /// <summary>
    ///     Tests that LiquidityPoolShareTrustlineAsset.Type property returns "pool_share".
    ///     Verifies the asset type identifier for liquidity pool share trustline assets.
    /// </summary>
    [TestMethod]
    public void TestType()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var trustlineAsset = (LiquidityPoolShareTrustlineAsset)TrustlineAsset.Create(
            LiquidityPoolParameters.Create(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30));
        Assert.AreEqual(trustlineAsset.Type, "pool_share");
    }

    /// <summary>
    ///     Tests that LiquidityPoolShareTrustlineAsset constructor throws ArgumentNullException when pool ID is null.
    ///     Validates that a valid LiquidityPoolId is required for creating pool share trustline assets.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestConstructor_NullId()
    {
        _ = new LiquidityPoolShareTrustlineAsset((LiquidityPoolId)null!);
    }

    /// <summary>
    ///     Tests that LiquidityPoolShareTrustlineAsset.Equals returns false when comparing with null.
    ///     Verifies null safety in the Equals implementation.
    /// </summary>
    [TestMethod]
    public void TestEquals_Null()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var trustlineAsset = (LiquidityPoolShareTrustlineAsset)TrustlineAsset.Create(
            LiquidityPoolParameters.Create(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30));
        Assert.IsFalse(trustlineAsset.Equals(null));
    }

    /// <summary>
    ///     Tests that LiquidityPoolShareTrustlineAsset.Equals returns false when comparing with incompatible types.
    ///     Verifies type safety in the Equals implementation for non-matching TrustlineAsset types.
    /// </summary>
    [TestMethod]
    public void TestEquals_NonMatchingType()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var trustlineAsset = (LiquidityPoolShareTrustlineAsset)TrustlineAsset.Create(
            LiquidityPoolParameters.Create(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30));
        var wrapper = new TrustlineAsset.Wrapper(Asset.Create($"USD:{keypair.AccountId}"));

        Assert.IsFalse(trustlineAsset.Equals(wrapper));
    }

    /// <summary>
    ///     Tests comparison ordering between LiquidityPoolShareTrustlineAsset and TrustlineAsset.Wrapper.
    ///     Verifies that pool share assets sort after regular asset wrappers (returns 1).
    /// </summary>
    [TestMethod]
    public void TestCompareTo_WithWrapper()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var trustlineAsset = (LiquidityPoolShareTrustlineAsset)TrustlineAsset.Create(
            LiquidityPoolParameters.Create(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30));
        var wrapper = new TrustlineAsset.Wrapper(Asset.Create($"USD:{keypair.AccountId}"));

        var comparison = trustlineAsset.CompareTo(wrapper);
        Assert.AreEqual(1, comparison);
    }

    /// <summary>
    ///     Tests comparison ordering between two LiquidityPoolShareTrustlineAsset instances.
    ///     Verifies that CompareTo correctly compares different liquidity pools based on their string representation.
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

        var trustlineAsset1 = (LiquidityPoolShareTrustlineAsset)TrustlineAsset.Create(
            LiquidityPoolParameters.Create(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30));
        var trustlineAsset2 = (LiquidityPoolShareTrustlineAsset)TrustlineAsset.Create(
            LiquidityPoolParameters.Create(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetC, 30));

        var comparison = trustlineAsset1.CompareTo(trustlineAsset2);
        Assert.IsTrue(comparison != 0);
    }

    /// <summary>
    ///     Tests ToXdrTrustLineAsset method for LiquidityPoolShareTrustlineAsset.
    ///     Verifies that the asset is correctly serialized to XDR format with ASSET_TYPE_POOL_SHARE discriminant
    ///     and includes the liquidity pool ID.
    /// </summary>
    [TestMethod]
    public void TestToXdrTrustLineAsset()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var trustlineAsset = (LiquidityPoolShareTrustlineAsset)TrustlineAsset.Create(
            LiquidityPoolParameters.Create(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30));

        var xdr = trustlineAsset.ToXdrTrustLineAsset();
        Assert.IsNotNull(xdr);
        Assert.AreEqual(AssetType.AssetTypeEnum.ASSET_TYPE_POOL_SHARE, xdr.Discriminant.InnerValue);
        Assert.IsNotNull(xdr.LiquidityPoolID);
    }

    /// <summary>
    ///     Tests that LiquidityPoolShareTrustlineAsset instances with the same pool ID produce the same hash code.
    ///     Verifies GetHashCode implementation for proper behavior in hash-based collections.
    /// </summary>
    [TestMethod]
    public void TestGetHashCode()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var trustlineAsset1 = (LiquidityPoolShareTrustlineAsset)TrustlineAsset.Create(
            LiquidityPoolParameters.Create(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30));
        var trustlineAsset2 = new LiquidityPoolShareTrustlineAsset(trustlineAsset1.Id);

        Assert.AreEqual(trustlineAsset1.GetHashCode(), trustlineAsset2.GetHashCode());
    }
}