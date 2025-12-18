using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.LiquidityPool;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Assets;

[TestClass]
public class ChangeTrustAssetTest
{
    /// <summary>
    ///     Tests ChangeTrustAsset creation from canonical form string (format: "CODE:ISSUER").
    ///     Verifies that the factory method correctly parses the canonical form and creates a Wrapper with the appropriate
    ///     asset.
    /// </summary>
    [TestMethod]
    public void TestCreateCanonical()
    {
        var keypair = KeyPair.Random();
        var changeTrustAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.Create($"USD:{keypair.AccountId}");
        Assert.AreEqual(changeTrustAsset.Asset.CanonicalName(), $"USD:{keypair.AccountId}");
    }

    /// <summary>
    ///     Tests ChangeTrustAsset creation from LiquidityPoolParameters and XDR round-trip conversion.
    ///     Verifies that liquidity pool share assets can be created, serialized to XDR, and deserialized back correctly.
    /// </summary>
    [TestMethod]
    public void TestCreateLiquidityPoolParameters()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var parameters =
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30);

        var liquidityPoolShareChangeTrustAsset =
            (LiquidityPoolShareChangeTrustAsset)ChangeTrustAsset.Create(parameters);
        var liquidityPoolShareChangeTrustAsset2 =
            (LiquidityPoolShareChangeTrustAsset)ChangeTrustAsset.FromXdr(liquidityPoolShareChangeTrustAsset.ToXdr());

        Assert.AreEqual(liquidityPoolShareChangeTrustAsset.Parameters.GetId(),
            liquidityPoolShareChangeTrustAsset2.Parameters.GetId());
    }

    /// <summary>
    ///     Tests ChangeTrustAsset creation from a TrustlineAsset.Wrapper.
    ///     Verifies interoperability between TrustlineAsset and ChangeTrustAsset by extracting the underlying asset.
    /// </summary>
    [TestMethod]
    public void TestCreateTrustlineWrapper()
    {
        var keypair = KeyPair.Random();
        var trustlineAsset = (TrustlineAsset.Wrapper)TrustlineAsset.Create("non-native", "USD", keypair.AccountId);
        var changeTrustAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.Create(trustlineAsset);

        Assert.AreEqual(trustlineAsset.Asset, changeTrustAsset.Asset);
    }

    /// <summary>
    ///     Tests ChangeTrustAsset creation for non-native assets using CreateNonNativeAsset helper method.
    ///     Verifies that credit assets are correctly created and wrapped.
    /// </summary>
    [TestMethod]
    public void TestCreateNonNative()
    {
        var keypair = KeyPair.Random();
        var changeTrustAsset =
            (ChangeTrustAsset.Wrapper)ChangeTrustAsset.CreateNonNativeAsset("USD", keypair.AccountId);
        Assert.AreEqual(changeTrustAsset.Asset.CanonicalName(), $"USD:{keypair.AccountId}");
    }

    /// <summary>
    ///     Tests deserialization of ChangeTrustAsset from XDR format for CreditAlphaNum4 assets.
    ///     Verifies that XDR data with ASSET_TYPE_CREDIT_ALPHANUM4 discriminant is correctly parsed.
    /// </summary>
    [TestMethod]
    public void TestFromXdrAlphaNum4()
    {
        var keypair = KeyPair.Random();

        var changeTrustAssetXdr = new XDR.ChangeTrustAsset();
        changeTrustAssetXdr.Discriminant = new XDR.AssetType
            { InnerValue = XDR.AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM4 };
        changeTrustAssetXdr.AlphaNum4 = Asset.Create($"USD:{keypair.AccountId}").ToXdr().AlphaNum4;

        var changeTrustAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.FromXdr(changeTrustAssetXdr);
        Assert.AreEqual(changeTrustAsset.Asset.CanonicalName(), $"USD:{keypair.AccountId}");
    }

    /// <summary>
    ///     Tests deserialization of ChangeTrustAsset from XDR format for CreditAlphaNum12 assets.
    ///     Verifies that XDR data with ASSET_TYPE_CREDIT_ALPHANUM12 discriminant is correctly parsed.
    /// </summary>
    [TestMethod]
    public void TestFromXdrAlphaNum12()
    {
        var keypair = KeyPair.Random();

        var changeTrustAssetXdr = new XDR.ChangeTrustAsset();
        changeTrustAssetXdr.Discriminant = new XDR.AssetType
            { InnerValue = XDR.AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM12 };
        changeTrustAssetXdr.AlphaNum12 = Asset.Create($"USDUSD:{keypair.AccountId}").ToXdr().AlphaNum12;

        var changeTrustAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.FromXdr(changeTrustAssetXdr);
        Assert.AreEqual(changeTrustAsset.Asset.CanonicalName(), $"USDUSD:{keypair.AccountId}");
    }

    /// <summary>
    ///     Tests deserialization of ChangeTrustAsset from XDR format for native assets.
    ///     Verifies that XDR data with ASSET_TYPE_NATIVE discriminant correctly creates a native asset wrapper.
    /// </summary>
    [TestMethod]
    public void TestFromXdrNative()
    {
        var changeTrustAssetXdr = new XDR.ChangeTrustAsset();
        changeTrustAssetXdr.Discriminant = new XDR.AssetType
            { InnerValue = XDR.AssetType.AssetTypeEnum.ASSET_TYPE_NATIVE };

        var trustlineAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.FromXdr(changeTrustAssetXdr);
        Assert.AreEqual(trustlineAsset.Asset.CanonicalName(), "native");
    }

    /// <summary>
    ///     Tests ChangeTrustAsset creation directly from an Asset instance.
    ///     Verifies that the factory method correctly wraps a regular Asset as a ChangeTrustAsset.
    /// </summary>
    [TestMethod]
    public void TestCreateWithAsset()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var changeTrustAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.Create(asset);
        Assert.AreEqual(asset, changeTrustAsset.Asset);
    }

    /// <summary>
    ///     Tests ChangeTrustAsset creation for liquidity pool shares using two assets and fee.
    ///     Verifies that a LiquidityPoolShareChangeTrustAsset is created with the correct pool parameters.
    /// </summary>
    [TestMethod]
    public void TestCreateWithAssetAAndAssetB()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var liquidityPoolShareChangeTrustAsset =
            (LiquidityPoolShareChangeTrustAsset)ChangeTrustAsset.Create(assetA, assetB, 30);
        Assert.IsNotNull(liquidityPoolShareChangeTrustAsset.Parameters);
        Assert.AreEqual("pool_share", liquidityPoolShareChangeTrustAsset.Type);
    }

    /// <summary>
    ///     Tests that ChangeTrustAsset.FromXdr throws ArgumentException for unknown asset types in XDR.
    ///     Validates error handling for malformed or future XDR data with unrecognized asset type discriminants.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestFromXdr_UnknownAssetType()
    {
        var changeTrustAssetXdr = new XDR.ChangeTrustAsset();
        changeTrustAssetXdr.Discriminant = new XDR.AssetType
            { InnerValue = (XDR.AssetType.AssetTypeEnum)999 };

        ChangeTrustAsset.FromXdr(changeTrustAssetXdr);
    }

    /// <summary>
    ///     Tests that ChangeTrustAsset.Wrapper instances with the same underlying asset are considered equal.
    ///     Verifies that Equals implementation correctly compares the wrapped Asset instances.
    /// </summary>
    [TestMethod]
    public void TestWrapperEquals_SameAsset()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper1 = new ChangeTrustAsset.Wrapper(asset);
        var wrapper2 = new ChangeTrustAsset.Wrapper(asset);
        Assert.IsTrue(wrapper1.Equals(wrapper2));
    }

    /// <summary>
    ///     Tests that ChangeTrustAsset.Wrapper instances with different underlying assets are not equal.
    ///     Verifies that Equals correctly distinguishes between different assets.
    /// </summary>
    [TestMethod]
    public void TestWrapperEquals_DifferentAsset()
    {
        var keypair1 = KeyPair.Random();
        var keypair2 = KeyPair.Random();
        var asset1 = Asset.Create($"USD:{keypair1.AccountId}");
        var asset2 = Asset.Create($"EUR:{keypair2.AccountId}");
        var wrapper1 = new ChangeTrustAsset.Wrapper(asset1);
        var wrapper2 = new ChangeTrustAsset.Wrapper(asset2);
        Assert.IsFalse(wrapper1.Equals(wrapper2));
    }

    /// <summary>
    ///     Tests that ChangeTrustAsset.Wrapper.Equals returns false when comparing with null.
    ///     Verifies null safety in the Equals implementation.
    /// </summary>
    [TestMethod]
    public void TestWrapperEquals_Null()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper = new ChangeTrustAsset.Wrapper(asset);
        Assert.IsFalse(wrapper.Equals(null));
    }

    /// <summary>
    ///     Tests comparison ordering between ChangeTrustAsset.Wrapper and LiquidityPoolShareChangeTrustAsset.
    ///     Verifies that regular asset wrappers sort before pool share assets (returns -1).
    /// </summary>
    [TestMethod]
    public void TestWrapperCompareTo_WithPoolShare()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper = new ChangeTrustAsset.Wrapper(asset);

        var keypair2 = KeyPair.Random();
        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"GBP:{keypair2.AccountId}");
        var poolShare = new LiquidityPoolShareChangeTrustAsset(assetA, assetB, 30);

        var comparison = wrapper.CompareTo(poolShare);
        Assert.AreEqual(-1, comparison);
    }

    /// <summary>
    ///     Tests comparison ordering between two ChangeTrustAsset.Wrapper instances.
    ///     Verifies that CompareTo delegates to the underlying Asset.CompareTo method.
    /// </summary>
    [TestMethod]
    public void TestWrapperCompareTo_WithWrapper()
    {
        var keypair = KeyPair.Random();
        var asset1 = Asset.Create($"EUR:{keypair.AccountId}");
        var asset2 = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper1 = new ChangeTrustAsset.Wrapper(asset1);
        var wrapper2 = new ChangeTrustAsset.Wrapper(asset2);

        var comparison = wrapper1.CompareTo(wrapper2);
        Assert.IsTrue(comparison != 0);
    }

    /// <summary>
    ///     Tests that ChangeTrustAsset.Wrapper instances with the same underlying asset produce the same hash code.
    ///     Verifies GetHashCode implementation for proper behavior in hash-based collections.
    /// </summary>
    [TestMethod]
    public void TestWrapperGetHashCode()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper1 = new ChangeTrustAsset.Wrapper(asset);
        var wrapper2 = new ChangeTrustAsset.Wrapper(asset);
        Assert.AreEqual(wrapper1.GetHashCode(), wrapper2.GetHashCode());
    }

    /// <summary>
    ///     Tests ChangeTrustAsset creation using the Create(string type, string code, string issuer) overload for native
    ///     assets.
    ///     Verifies that passing "native" as the type correctly creates a native asset wrapper.
    /// </summary>
    [TestMethod]
    public void TestCreateWithTypeCodeIssuer_Native()
    {
        var changeTrustAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.Create("native", null!, null!);
        Assert.IsTrue(changeTrustAsset.Asset is AssetTypeNative);
        Assert.AreEqual("native", changeTrustAsset.Asset.CanonicalName());
    }

    /// <summary>
    ///     Tests ChangeTrustAsset creation using the Create(string type, string code, string issuer) overload for non-native
    ///     assets.
    ///     Verifies that passing "non-native" as the type correctly creates a credit asset wrapper.
    /// </summary>
    [TestMethod]
    public void TestCreateWithTypeCodeIssuer_NonNative()
    {
        var keypair = KeyPair.Random();
        var changeTrustAsset =
            (ChangeTrustAsset.Wrapper)ChangeTrustAsset.Create("non-native", "USD", keypair.AccountId);
        Assert.IsTrue(changeTrustAsset.Asset is AssetTypeCreditAlphaNum);
        Assert.AreEqual($"USD:{keypair.AccountId}", changeTrustAsset.Asset.CanonicalName());
    }

    /// <summary>
    ///     Tests XDR serialization and deserialization round-trip for ChangeTrustAsset.Wrapper with native assets.
    ///     Verifies that native assets can be converted to XDR format and back without data loss.
    /// </summary>
    [TestMethod]
    public void TestWrapperToXdr_Native()
    {
        var wrapper = new ChangeTrustAsset.Wrapper(new AssetTypeNative());
        var xdr = wrapper.ToXdr();
        var roundTripWrapper = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.FromXdr(xdr);

        Assert.IsTrue(roundTripWrapper.Asset is AssetTypeNative);
        Assert.AreEqual("native", roundTripWrapper.Asset.CanonicalName());
    }

    /// <summary>
    ///     Tests XDR serialization and deserialization round-trip for ChangeTrustAsset.Wrapper with CreditAlphaNum4 assets.
    ///     Verifies that 4-character asset codes are correctly preserved through XDR conversion.
    /// </summary>
    [TestMethod]
    public void TestWrapperToXdr_CreditAlphaNum4()
    {
        var keypair = KeyPair.Random();
        var asset = new AssetTypeCreditAlphaNum4("USD", keypair.AccountId);
        var wrapper = new ChangeTrustAsset.Wrapper(asset);
        var xdr = wrapper.ToXdr();
        var roundTripWrapper = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.FromXdr(xdr);

        Assert.IsTrue(roundTripWrapper.Asset is AssetTypeCreditAlphaNum4);
        Assert.AreEqual($"USD:{keypair.AccountId}", roundTripWrapper.Asset.CanonicalName());
    }

    /// <summary>
    ///     Tests XDR serialization and deserialization round-trip for ChangeTrustAsset.Wrapper with CreditAlphaNum12 assets.
    ///     Verifies that 5-12 character asset codes are correctly preserved through XDR conversion.
    /// </summary>
    [TestMethod]
    public void TestWrapperToXdr_CreditAlphaNum12()
    {
        var keypair = KeyPair.Random();
        var asset = new AssetTypeCreditAlphaNum12("TESTTEST", keypair.AccountId);
        var wrapper = new ChangeTrustAsset.Wrapper(asset);
        var xdr = wrapper.ToXdr();
        var roundTripWrapper = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.FromXdr(xdr);

        Assert.IsTrue(roundTripWrapper.Asset is AssetTypeCreditAlphaNum12);
        Assert.AreEqual($"TESTTEST:{keypair.AccountId}", roundTripWrapper.Asset.CanonicalName());
    }
}