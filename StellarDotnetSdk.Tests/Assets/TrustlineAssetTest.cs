using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.LiquidityPool;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Assets;

[TestClass]
public class TrustlineAssetTest
{
    /// <summary>
    /// Tests TrustlineAsset creation from canonical form string (format: "CODE:ISSUER").
    /// Verifies that the factory method correctly parses the canonical form and creates a Wrapper with the appropriate asset.
    /// </summary>
    [TestMethod]
    public void TestCreateCanonicalName()
    {
        var keypair = KeyPair.Random();
        var trustlineAsset = TrustlineAsset.Create($"USD:{keypair.AccountId}");
        Assert.AreEqual(((TrustlineAsset.Wrapper)trustlineAsset).Asset.CanonicalName(), $"USD:{keypair.AccountId}");
    }

    /// <summary>
    /// Tests TrustlineAsset creation using the Create(string type, string code, string issuer) overload.
    /// Verifies that non-native assets are correctly created and wrapped.
    /// </summary>
    [TestMethod]
    public void TestCreate()
    {
        var keypair = KeyPair.Random();
        var trustlineAsset = TrustlineAsset.Create("non-native", "USD", keypair.AccountId);
        Assert.AreEqual(((TrustlineAsset.Wrapper)trustlineAsset).Asset.CanonicalName(), $"USD:{keypair.AccountId}");
    }

    /// <summary>
    /// Tests TrustlineAsset creation from LiquidityPoolParameters.
    /// Verifies that liquidity pool share trustline assets can be created and their pool IDs are correctly preserved.
    /// </summary>
    [TestMethod]
    public void TestCreateParameters()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var trustlineAsset = (LiquidityPoolShareTrustlineAsset)TrustlineAsset.Create(
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30));
        var trustlineAsset2 = new LiquidityPoolShareTrustlineAsset(trustlineAsset.Id);
        Assert.AreEqual(trustlineAsset.Id, trustlineAsset2.Id);
    }

    /// <summary>
    /// Tests TrustlineAsset creation from a LiquidityPoolShareChangeTrustAsset.
    /// Verifies interoperability between ChangeTrustAsset and TrustlineAsset for liquidity pool shares.
    /// </summary>
    [TestMethod]
    public void TestCreateShareChangeTrust()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var liquidityPoolParameters =
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30);
        var trustlineAsset =
            (LiquidityPoolShareTrustlineAsset)TrustlineAsset.Create(
                new LiquidityPoolShareChangeTrustAsset(liquidityPoolParameters));
        var trustlineAsset2 = new LiquidityPoolShareTrustlineAsset(trustlineAsset.Id);
        Assert.AreEqual(trustlineAsset.Id, trustlineAsset2.Id);
    }

    /// <summary>
    /// Tests TrustlineAsset creation from a LiquidityPoolId.
    /// Verifies that trustline assets can be created directly from a pool ID without requiring full parameters.
    /// </summary>
    [TestMethod]
    public void TestCreateLiquidityPoolId()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var liquidityPoolParameters =
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30);
        var trustlineAsset =
            (LiquidityPoolShareTrustlineAsset)TrustlineAsset.Create(
                new LiquidityPoolShareChangeTrustAsset(liquidityPoolParameters));
        var trustlineAsset2 = (LiquidityPoolShareTrustlineAsset)TrustlineAsset.Create(trustlineAsset.Id);
        Assert.AreEqual(trustlineAsset.Id, trustlineAsset2.Id);
    }

    /// <summary>
    /// Tests deserialization of TrustlineAsset from XDR format for CreditAlphaNum4 assets.
    /// Verifies that XDR data with ASSET_TYPE_CREDIT_ALPHANUM4 discriminant is correctly parsed.
    /// </summary>
    [TestMethod]
    public void TestFromXdrAlphaNum4()
    {
        var keypair = KeyPair.Random();

        var trustlineAssetNativeXdr = new XDR.TrustLineAsset
        {
            Discriminant = new XDR.AssetType { InnerValue = XDR.AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM4 },
            AlphaNum4 = Asset.Create($"USD:{keypair.AccountId}").ToXdr().AlphaNum4,
        };

        var trustlineAsset = (TrustlineAsset.Wrapper)TrustlineAsset.FromXdr(trustlineAssetNativeXdr);
        Assert.AreEqual(trustlineAsset.Asset.CanonicalName(), $"USD:{keypair.AccountId}");
    }

    /// <summary>
    /// Tests deserialization of TrustlineAsset from XDR format for CreditAlphaNum12 assets.
    /// Verifies that XDR data with ASSET_TYPE_CREDIT_ALPHANUM12 discriminant is correctly parsed.
    /// </summary>
    [TestMethod]
    public void TestFromXdrAlphaNum12()
    {
        var keypair = KeyPair.Random();

        var trustlineAssetNativeXdr = new XDR.TrustLineAsset
        {
            Discriminant = XDR.AssetType.Create(XDR.AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM12),
            AlphaNum12 = Asset.Create($"USDUSD:{keypair.AccountId}").ToXdr().AlphaNum12,
        };

        var trustlineAsset = (TrustlineAsset.Wrapper)TrustlineAsset.FromXdr(trustlineAssetNativeXdr);
        Assert.AreEqual(trustlineAsset.Asset.CanonicalName(), $"USDUSD:{keypair.AccountId}");
    }

    /// <summary>
    /// Tests deserialization of TrustlineAsset from XDR format for native assets.
    /// Verifies that XDR data with ASSET_TYPE_NATIVE discriminant correctly creates a native asset wrapper.
    /// </summary>
    [TestMethod]
    public void TestFromXdrNative()
    {
        var trustlineAssetNativeXdr = new XDR.TrustLineAsset
        {
            Discriminant = XDR.AssetType.Create(XDR.AssetType.AssetTypeEnum.ASSET_TYPE_NATIVE),
        };

        var trustlineAsset = (TrustlineAsset.Wrapper)TrustlineAsset.FromXdr(trustlineAssetNativeXdr);
        Assert.AreEqual(trustlineAsset.Asset.CanonicalName(), "native");
    }

    /// <summary>
    /// Tests XDR round-trip conversion for LiquidityPoolShareTrustlineAsset.
    /// Verifies that liquidity pool share trustline assets can be serialized to XDR and deserialized back correctly,
    /// preserving the pool ID hash.
    /// </summary>
    [TestMethod]
    public void TestLiquidityPoolShareTrustlineAsset()
    {
        var asset = (LiquidityPoolShareTrustlineAsset)TrustlineAsset.Create(
            new LiquidityPoolConstantProductParameters(new AssetTypeNative(),
                Asset.CreateNonNativeAsset(
                    "VNDT",
                    "GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7"),
                1000));
        var xdrAsset = asset.ToXdr();
        var decodedAsset = (LiquidityPoolShareTrustlineAsset)TrustlineAsset.FromXdr(xdrAsset);
        CollectionAssert.AreEqual(asset.Id.Hash, decodedAsset.Id.Hash);
    }

    /// <summary>
    /// Tests TrustlineAsset creation from a ChangeTrustAsset.Wrapper.
    /// Verifies interoperability between ChangeTrustAsset and TrustlineAsset by extracting the underlying asset.
    /// </summary>
    [TestMethod]
    public void TestCreateWithChangeTrustAssetWrapper()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var changeTrustWrapper = new ChangeTrustAsset.Wrapper(asset);
        var trustlineAsset = (TrustlineAsset.Wrapper)TrustlineAsset.Create(changeTrustWrapper);
        Assert.AreEqual(asset, trustlineAsset.Asset);
    }

    /// <summary>
    /// Tests TrustlineAsset creation for non-native assets using CreateNonNativeAsset helper method.
    /// Verifies that credit assets are correctly created and wrapped.
    /// </summary>
    [TestMethod]
    public void TestCreateNonNativeAsset()
    {
        var keypair = KeyPair.Random();
        var trustlineAsset = (TrustlineAsset.Wrapper)TrustlineAsset.CreateNonNativeAsset("USD", keypair.AccountId);
        Assert.AreEqual($"USD:{keypair.AccountId}", trustlineAsset.Asset.CanonicalName());
    }

    /// <summary>
    /// Tests that TrustlineAsset.FromXdr throws ArgumentException for unknown asset types in XDR.
    /// Validates error handling for malformed or future XDR data with unrecognized asset type discriminants.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestFromXdr_UnknownAssetType()
    {
        var trustlineAssetXdr = new XDR.TrustLineAsset
        {
            Discriminant = new XDR.AssetType { InnerValue = (XDR.AssetType.AssetTypeEnum)999 }
        };
        TrustlineAsset.FromXdr(trustlineAssetXdr);
    }

    /// <summary>
    /// Tests that TrustlineAsset.Wrapper.Equals returns false when comparing with null.
    /// Verifies null safety in the Equals implementation.
    /// </summary>
    [TestMethod]
    public void TestWrapperEquals_Null()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper = new TrustlineAsset.Wrapper(asset);
        Assert.IsFalse(wrapper.Equals(null));
    }

    /// <summary>
    /// Tests that TrustlineAsset.Wrapper.Equals returns false when comparing with incompatible types.
    /// Verifies type safety in the Equals implementation for non-matching object types.
    /// </summary>
    [TestMethod]
    public void TestWrapperEquals_TypeMismatch()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper = new TrustlineAsset.Wrapper(asset);
        Assert.IsFalse(wrapper.Equals(new object()));
    }

    /// <summary>
    /// Tests that TrustlineAsset.Wrapper instances with the same underlying asset are considered equal.
    /// Verifies that Equals implementation correctly compares the wrapped Asset instances.
    /// </summary>
    [TestMethod]
    public void TestWrapperEquals_SameAsset()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper1 = new TrustlineAsset.Wrapper(asset);
        var wrapper2 = new TrustlineAsset.Wrapper(asset);
        Assert.IsTrue(wrapper1.Equals(wrapper2));
    }

    /// <summary>
    /// Tests that TrustlineAsset.Wrapper instances with different underlying assets are not equal.
    /// Verifies that Equals correctly distinguishes between different assets.
    /// </summary>
    [TestMethod]
    public void TestWrapperEquals_DifferentAsset()
    {
        var keypair1 = KeyPair.Random();
        var keypair2 = KeyPair.Random();
        var asset1 = Asset.Create($"USD:{keypair1.AccountId}");
        var asset2 = Asset.Create($"EUR:{keypair2.AccountId}");
        var wrapper1 = new TrustlineAsset.Wrapper(asset1);
        var wrapper2 = new TrustlineAsset.Wrapper(asset2);
        Assert.IsFalse(wrapper1.Equals(wrapper2));
    }

    /// <summary>
    /// Tests comparison ordering between TrustlineAsset.Wrapper and LiquidityPoolShareTrustlineAsset.
    /// Verifies that regular asset wrappers sort before pool share assets (returns -1).
    /// </summary>
    [TestMethod]
    public void TestWrapperCompareTo_WithPoolShare()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper = new TrustlineAsset.Wrapper(asset);

        var keypair2 = KeyPair.Random();
        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"GBP:{keypair2.AccountId}");
        var poolShare = new LiquidityPoolShareTrustlineAsset(
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30));

        var comparison = wrapper.CompareTo(poolShare);
        Assert.AreEqual(-1, comparison);
    }

    /// <summary>
    /// Tests comparison ordering between two TrustlineAsset.Wrapper instances.
    /// Verifies that CompareTo delegates to the underlying Asset.CompareTo method.
    /// </summary>
    [TestMethod]
    public void TestWrapperCompareTo_WithWrapper()
    {
        var keypair = KeyPair.Random();
        var asset1 = Asset.Create($"EUR:{keypair.AccountId}");
        var asset2 = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper1 = new TrustlineAsset.Wrapper(asset1);
        var wrapper2 = new TrustlineAsset.Wrapper(asset2);

        var comparison = wrapper1.CompareTo(wrapper2);
        Assert.IsTrue(comparison != 0);
    }

    /// <summary>
    /// Tests XDR serialization and deserialization round-trip for TrustlineAsset.Wrapper with credit assets.
    /// Verifies that credit assets can be converted to XDR format and back without data loss.
    /// </summary>
    [TestMethod]
    public void TestToXdr_Wrapper()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        TrustlineAsset trustlineAsset = new TrustlineAsset.Wrapper(asset);
        var xdr = trustlineAsset.ToXdr();
        var roundTripAsset = (TrustlineAsset.Wrapper)TrustlineAsset.FromXdr(xdr);
        
        Assert.IsTrue(roundTripAsset.Asset is AssetTypeCreditAlphaNum);
        Assert.AreEqual($"USD:{keypair.AccountId}", roundTripAsset.Asset.CanonicalName());
    }

    /// <summary>
    /// Tests XDR serialization and deserialization round-trip for TrustlineAsset.Wrapper with native assets.
    /// Verifies that native assets can be converted to XDR format and back without data loss.
    /// </summary>
    [TestMethod]
    public void TestToXdr_Wrapper_Native()
    {
        var nativeAsset = new AssetTypeNative();
        TrustlineAsset trustlineAsset = new TrustlineAsset.Wrapper(nativeAsset);
        var xdr = trustlineAsset.ToXdr();
        var roundTripAsset = (TrustlineAsset.Wrapper)TrustlineAsset.FromXdr(xdr);
        
        Assert.IsTrue(roundTripAsset.Asset is AssetTypeNative);
        Assert.AreEqual("native", roundTripAsset.Asset.CanonicalName());
    }

    /// <summary>
    /// Tests XDR serialization and deserialization round-trip for LiquidityPoolShareTrustlineAsset.
    /// Verifies that liquidity pool share trustline assets can be converted to XDR format and back,
    /// preserving the pool ID.
    /// </summary>
    [TestMethod]
    public void TestToXdr_LiquidityPoolShareTrustlineAsset()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();
        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");
        
        var parameters = LiquidityPoolParameters.Create(
            XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
            assetA, assetB, 30);
        TrustlineAsset trustlineAsset = new LiquidityPoolShareTrustlineAsset(parameters);
        var xdr = trustlineAsset.ToXdr();
        var roundTripAsset = (LiquidityPoolShareTrustlineAsset)TrustlineAsset.FromXdr(xdr);
        
        Assert.IsInstanceOfType(roundTripAsset, typeof(LiquidityPoolShareTrustlineAsset));
        Assert.AreEqual(parameters.GetId(), roundTripAsset.Id);
    }

    /// <summary>
    /// Tests that TrustlineAsset.ToXdr throws InvalidOperationException for unknown asset types.
    /// Verifies the default case in the switch expression that handles unexpected TrustlineAsset implementations.
    /// This test uses a test-only subclass to exercise the otherwise unreachable exception path.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void TestToXdr_UnknownType_ThrowsException()
    {
        // Create a test-only subclass that doesn't match either pattern in the switch expression
        // This tests the default case (_ => throw ...) which is otherwise unreachable
        var unknownTrustlineAsset = new UnknownTrustlineAssetForTesting();
        unknownTrustlineAsset.ToXdr();
    }

    // Test-only class to exercise the default case in ToXdr() switch expression
    private class UnknownTrustlineAssetForTesting : TrustlineAsset
    {
        public override string Type => "unknown_test_type";

        public override bool Equals(object? obj)
        {
            return obj is UnknownTrustlineAssetForTesting;
        }

        public override int CompareTo(TrustlineAsset asset)
        {
            return 0;
        }
    }
}