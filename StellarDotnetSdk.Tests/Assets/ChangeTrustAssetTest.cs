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
    public void Create_WithCanonicalForm_ReturnsWrapperWithCorrectAsset()
    {
        // Arrange
        var keypair = KeyPair.Random();

        // Act
        var changeTrustAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.Create($"USD:{keypair.AccountId}");

        // Assert
        Assert.AreEqual(changeTrustAsset.Asset.CanonicalName(), $"USD:{keypair.AccountId}");
    }

    /// <summary>
    ///     Tests ChangeTrustAsset creation from LiquidityPoolParameters and XDR round-trip conversion.
    ///     Verifies that liquidity pool share assets can be created, serialized to XDR, and deserialized back correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithLiquidityPoolParameters_RoundTripsCorrectly()
    {
        // Arrange
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();
        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");
        var parameters =
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30);

        // Act
        var liquidityPoolShareChangeTrustAsset =
            (LiquidityPoolShareChangeTrustAsset)ChangeTrustAsset.Create(parameters);
        var liquidityPoolShareChangeTrustAsset2 =
            (LiquidityPoolShareChangeTrustAsset)ChangeTrustAsset.FromXdr(liquidityPoolShareChangeTrustAsset.ToXdr());

        // Assert
        Assert.AreEqual(liquidityPoolShareChangeTrustAsset.Parameters.GetId(),
            liquidityPoolShareChangeTrustAsset2.Parameters.GetId());
    }

    /// <summary>
    ///     Tests ChangeTrustAsset creation from a TrustlineAsset.Wrapper.
    ///     Verifies interoperability between TrustlineAsset and ChangeTrustAsset by extracting the underlying asset.
    /// </summary>
    [TestMethod]
    public void Create_WithTrustlineWrapper_ReturnsWrapperWithSameAsset()
    {
        // Arrange
        var keypair = KeyPair.Random();
        var trustlineAsset = (TrustlineAsset.Wrapper)TrustlineAsset.Create("non-native", "USD", keypair.AccountId);

        // Act
        var changeTrustAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.Create(trustlineAsset);

        // Assert
        Assert.AreEqual(trustlineAsset.Asset, changeTrustAsset.Asset);
    }

    /// <summary>
    ///     Tests ChangeTrustAsset creation for non-native assets using CreateNonNativeAsset helper method.
    ///     Verifies that credit assets are correctly created and wrapped.
    /// </summary>
    [TestMethod]
    public void CreateNonNativeAsset_WithCodeAndIssuer_ReturnsWrapperWithCorrectAsset()
    {
        // Arrange
        var keypair = KeyPair.Random();

        // Act
        var changeTrustAsset =
            (ChangeTrustAsset.Wrapper)ChangeTrustAsset.CreateNonNativeAsset("USD", keypair.AccountId);

        // Assert
        Assert.AreEqual(changeTrustAsset.Asset.CanonicalName(), $"USD:{keypair.AccountId}");
    }

    /// <summary>
    ///     Tests deserialization of ChangeTrustAsset from XDR format for CreditAlphaNum4 assets.
    ///     Verifies that XDR data with ASSET_TYPE_CREDIT_ALPHANUM4 discriminant is correctly parsed.
    /// </summary>
    [TestMethod]
    public void FromXdr_WithCreditAlphaNum4_ReturnsWrapperWithCorrectAsset()
    {
        // Arrange
        var keypair = KeyPair.Random();
        var changeTrustAssetXdr = new XDR.ChangeTrustAsset();
        changeTrustAssetXdr.Discriminant = new XDR.AssetType
            { InnerValue = XDR.AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM4 };
        changeTrustAssetXdr.AlphaNum4 = Asset.Create($"USD:{keypair.AccountId}").ToXdr().AlphaNum4;

        // Act
        var changeTrustAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.FromXdr(changeTrustAssetXdr);

        // Assert
        Assert.AreEqual(changeTrustAsset.Asset.CanonicalName(), $"USD:{keypair.AccountId}");
    }

    /// <summary>
    ///     Tests deserialization of ChangeTrustAsset from XDR format for CreditAlphaNum12 assets.
    ///     Verifies that XDR data with ASSET_TYPE_CREDIT_ALPHANUM12 discriminant is correctly parsed.
    /// </summary>
    [TestMethod]
    public void FromXdr_WithCreditAlphaNum12_ReturnsWrapperWithCorrectAsset()
    {
        // Arrange
        var keypair = KeyPair.Random();
        var changeTrustAssetXdr = new XDR.ChangeTrustAsset();
        changeTrustAssetXdr.Discriminant = new XDR.AssetType
            { InnerValue = XDR.AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM12 };
        changeTrustAssetXdr.AlphaNum12 = Asset.Create($"USDUSD:{keypair.AccountId}").ToXdr().AlphaNum12;

        // Act
        var changeTrustAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.FromXdr(changeTrustAssetXdr);

        // Assert
        Assert.AreEqual(changeTrustAsset.Asset.CanonicalName(), $"USDUSD:{keypair.AccountId}");
    }

    /// <summary>
    ///     Tests deserialization of ChangeTrustAsset from XDR format for native assets.
    ///     Verifies that XDR data with ASSET_TYPE_NATIVE discriminant correctly creates a native asset wrapper.
    /// </summary>
    [TestMethod]
    public void FromXdr_WithNativeAsset_ReturnsWrapperWithNativeAsset()
    {
        // Arrange
        var changeTrustAssetXdr = new XDR.ChangeTrustAsset();
        changeTrustAssetXdr.Discriminant = new XDR.AssetType
            { InnerValue = XDR.AssetType.AssetTypeEnum.ASSET_TYPE_NATIVE };

        // Act
        var trustlineAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.FromXdr(changeTrustAssetXdr);

        // Assert
        Assert.AreEqual(trustlineAsset.Asset.CanonicalName(), "native");
    }

    /// <summary>
    ///     Tests ChangeTrustAsset creation directly from an Asset instance.
    ///     Verifies that the factory method correctly wraps a regular Asset as a ChangeTrustAsset.
    /// </summary>
    [TestMethod]
    public void Create_WithAsset_ReturnsWrapperWithSameAsset()
    {
        // Arrange
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");

        // Act
        var changeTrustAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.Create(asset);

        // Assert
        Assert.AreEqual(asset, changeTrustAsset.Asset);
    }

    /// <summary>
    ///     Tests ChangeTrustAsset creation for liquidity pool shares using two assets and fee.
    ///     Verifies that a LiquidityPoolShareChangeTrustAsset is created with the correct pool parameters.
    /// </summary>
    [TestMethod]
    public void Create_WithAssetAAndAssetB_ReturnsLiquidityPoolShareChangeTrustAsset()
    {
        // Arrange
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();
        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        // Act
        var liquidityPoolShareChangeTrustAsset =
            (LiquidityPoolShareChangeTrustAsset)ChangeTrustAsset.Create(assetA, assetB, 30);

        // Assert
        Assert.IsNotNull(liquidityPoolShareChangeTrustAsset.Parameters);
        Assert.AreEqual("pool_share", liquidityPoolShareChangeTrustAsset.Type);
    }

    /// <summary>
    ///     Tests that ChangeTrustAsset.FromXdr throws ArgumentException for unknown asset types in XDR.
    ///     Validates error handling for malformed or future XDR data with unrecognized asset type discriminants.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void FromXdr_WithUnknownAssetType_ThrowsArgumentException()
    {
        // Arrange
        var changeTrustAssetXdr = new XDR.ChangeTrustAsset();
        changeTrustAssetXdr.Discriminant = new XDR.AssetType
            { InnerValue = (XDR.AssetType.AssetTypeEnum)999 };

        // Act & Assert
        ChangeTrustAsset.FromXdr(changeTrustAssetXdr);
    }

    /// <summary>
    ///     Tests that ChangeTrustAsset.Wrapper instances with the same underlying asset are considered equal.
    ///     Verifies that Equals implementation correctly compares the wrapped Asset instances.
    /// </summary>
    [TestMethod]
    public void WrapperEquals_WithSameAsset_ReturnsTrue()
    {
        // Arrange
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper1 = new ChangeTrustAsset.Wrapper(asset);
        var wrapper2 = new ChangeTrustAsset.Wrapper(asset);

        // Act & Assert
        Assert.IsTrue(wrapper1.Equals(wrapper2));
    }

    /// <summary>
    ///     Tests that ChangeTrustAsset.Wrapper instances with different underlying assets are not equal.
    ///     Verifies that Equals correctly distinguishes between different assets.
    /// </summary>
    [TestMethod]
    public void WrapperEquals_WithDifferentAsset_ReturnsFalse()
    {
        // Arrange
        var keypair1 = KeyPair.Random();
        var keypair2 = KeyPair.Random();
        var asset1 = Asset.Create($"USD:{keypair1.AccountId}");
        var asset2 = Asset.Create($"EUR:{keypair2.AccountId}");
        var wrapper1 = new ChangeTrustAsset.Wrapper(asset1);
        var wrapper2 = new ChangeTrustAsset.Wrapper(asset2);

        // Act & Assert
        Assert.IsFalse(wrapper1.Equals(wrapper2));
    }

    /// <summary>
    ///     Tests that ChangeTrustAsset.Wrapper.Equals returns false when comparing with null.
    ///     Verifies null safety in the Equals implementation.
    /// </summary>
    [TestMethod]
    public void WrapperEquals_WithNull_ReturnsFalse()
    {
        // Arrange
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper = new ChangeTrustAsset.Wrapper(asset);

        // Act & Assert
        Assert.IsFalse(wrapper.Equals(null));
    }

    /// <summary>
    ///     Tests comparison ordering between ChangeTrustAsset.Wrapper and LiquidityPoolShareChangeTrustAsset.
    ///     Verifies that regular asset wrappers sort before pool share assets (returns -1).
    /// </summary>
    [TestMethod]
    public void WrapperCompareTo_WithPoolShare_ReturnsNegativeOne()
    {
        // Arrange
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper = new ChangeTrustAsset.Wrapper(asset);
        var keypair2 = KeyPair.Random();
        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"GBP:{keypair2.AccountId}");
        var poolShare = new LiquidityPoolShareChangeTrustAsset(assetA, assetB, 30);

        // Act
        var comparison = wrapper.CompareTo(poolShare);

        // Assert
        Assert.AreEqual(-1, comparison);
    }

    /// <summary>
    ///     Tests comparison ordering between two ChangeTrustAsset.Wrapper instances.
    ///     Verifies that CompareTo delegates to the underlying Asset.CompareTo method.
    /// </summary>
    [TestMethod]
    public void WrapperCompareTo_WithWrapper_ReturnsNonZero()
    {
        // Arrange
        var keypair = KeyPair.Random();
        var asset1 = Asset.Create($"EUR:{keypair.AccountId}");
        var asset2 = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper1 = new ChangeTrustAsset.Wrapper(asset1);
        var wrapper2 = new ChangeTrustAsset.Wrapper(asset2);

        // Act
        var comparison = wrapper1.CompareTo(wrapper2);

        // Assert
        Assert.IsTrue(comparison != 0);
    }

    /// <summary>
    ///     Tests that ChangeTrustAsset.Wrapper instances with the same underlying asset produce the same hash code.
    ///     Verifies GetHashCode implementation for proper behavior in hash-based collections.
    /// </summary>
    [TestMethod]
    public void WrapperGetHashCode_WithSameAsset_ReturnsSameHashCode()
    {
        // Arrange
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper1 = new ChangeTrustAsset.Wrapper(asset);
        var wrapper2 = new ChangeTrustAsset.Wrapper(asset);

        // Act & Assert
        Assert.AreEqual(wrapper1.GetHashCode(), wrapper2.GetHashCode());
    }

    /// <summary>
    ///     Tests ChangeTrustAsset creation using the Create(string type, string code, string issuer) overload for native
    ///     assets.
    ///     Verifies that passing "native" as the type correctly creates a native asset wrapper.
    /// </summary>
    [TestMethod]
    public void Create_WithTypeNative_ReturnsWrapperWithNativeAsset()
    {
        // Arrange & Act
        var changeTrustAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.Create("native", null!, null!);

        // Assert
        Assert.IsTrue(changeTrustAsset.Asset is AssetTypeNative);
        Assert.AreEqual("native", changeTrustAsset.Asset.CanonicalName());
    }

    /// <summary>
    ///     Tests ChangeTrustAsset creation using the Create(string type, string code, string issuer) overload for non-native
    ///     assets.
    ///     Verifies that passing "non-native" as the type correctly creates a credit asset wrapper.
    /// </summary>
    [TestMethod]
    public void Create_WithTypeNonNative_ReturnsWrapperWithCreditAsset()
    {
        // Arrange
        var keypair = KeyPair.Random();

        // Act
        var changeTrustAsset =
            (ChangeTrustAsset.Wrapper)ChangeTrustAsset.Create("non-native", "USD", keypair.AccountId);

        // Assert
        Assert.IsTrue(changeTrustAsset.Asset is AssetTypeCreditAlphaNum);
        Assert.AreEqual($"USD:{keypair.AccountId}", changeTrustAsset.Asset.CanonicalName());
    }

    /// <summary>
    ///     Tests XDR serialization and deserialization round-trip for ChangeTrustAsset.Wrapper with native assets.
    ///     Verifies that native assets can be converted to XDR format and back without data loss.
    /// </summary>
    [TestMethod]
    public void WrapperToXdr_WithNativeAsset_RoundTripsCorrectly()
    {
        // Arrange
        var wrapper = new ChangeTrustAsset.Wrapper(new AssetTypeNative());

        // Act
        var xdr = wrapper.ToXdr();
        var roundTripWrapper = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.FromXdr(xdr);

        // Assert
        Assert.IsTrue(roundTripWrapper.Asset is AssetTypeNative);
        Assert.AreEqual("native", roundTripWrapper.Asset.CanonicalName());
    }

    /// <summary>
    ///     Tests XDR serialization and deserialization round-trip for ChangeTrustAsset.Wrapper with CreditAlphaNum4 assets.
    ///     Verifies that 4-character asset codes are correctly preserved through XDR conversion.
    /// </summary>
    [TestMethod]
    public void WrapperToXdr_WithCreditAlphaNum4_RoundTripsCorrectly()
    {
        // Arrange
        var keypair = KeyPair.Random();
        var asset = new AssetTypeCreditAlphaNum4("USD", keypair.AccountId);
        var wrapper = new ChangeTrustAsset.Wrapper(asset);

        // Act
        var xdr = wrapper.ToXdr();
        var roundTripWrapper = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.FromXdr(xdr);

        // Assert
        Assert.IsTrue(roundTripWrapper.Asset is AssetTypeCreditAlphaNum4);
        Assert.AreEqual($"USD:{keypair.AccountId}", roundTripWrapper.Asset.CanonicalName());
    }

    /// <summary>
    ///     Tests XDR serialization and deserialization round-trip for ChangeTrustAsset.Wrapper with CreditAlphaNum12 assets.
    ///     Verifies that 5-12 character asset codes are correctly preserved through XDR conversion.
    /// </summary>
    [TestMethod]
    public void WrapperToXdr_WithCreditAlphaNum12_RoundTripsCorrectly()
    {
        // Arrange
        var keypair = KeyPair.Random();
        var asset = new AssetTypeCreditAlphaNum12("TESTTEST", keypair.AccountId);
        var wrapper = new ChangeTrustAsset.Wrapper(asset);

        // Act
        var xdr = wrapper.ToXdr();
        var roundTripWrapper = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.FromXdr(xdr);

        // Assert
        Assert.IsTrue(roundTripWrapper.Asset is AssetTypeCreditAlphaNum12);
        Assert.AreEqual($"TESTTEST:{keypair.AccountId}", roundTripWrapper.Asset.CanonicalName());
    }
}