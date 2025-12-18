using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Tests.Assets;

[TestClass]
public class AssetAmountTest
{
    /// <summary>
    ///     Tests AssetAmount creation with an asset and amount string.
    ///     Verifies that both Asset and Amount properties are correctly stored and accessible.
    /// </summary>
    [TestMethod]
    public void Constructor_WithAssetAndAmount_SetsPropertiesCorrectly()
    {
        // Arrange
        var issuer = KeyPair.Random();

        // Act
        var assetAmount = new AssetAmount(Asset.CreateNonNativeAsset("TEST", issuer.AccountId), "100");

        // Assert
        Assert.AreEqual(assetAmount.Asset.CanonicalName(), $"TEST:{issuer.AccountId}");
        Assert.AreEqual(assetAmount.Amount, "100");
    }

    /// <summary>
    ///     Tests that AssetAmount instances with the same asset and amount are considered equal.
    ///     Verifies both Equals and GetHashCode implementations for proper behavior in collections.
    /// </summary>
    [TestMethod]
    public void Equals_WithSameAssetAndAmount_ReturnsTrue()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var assetAmount = new AssetAmount(Asset.CreateNonNativeAsset("TEST", issuer.AccountId), "100");
        var assetAmount2 = new AssetAmount(Asset.CreateNonNativeAsset("TEST", issuer.AccountId), "100");

        // Act & Assert
        Assert.AreEqual(assetAmount, assetAmount2);
        Assert.AreEqual(assetAmount.GetHashCode(), assetAmount2.GetHashCode());
    }

    /// <summary>
    ///     Tests that AssetAmount.Equals returns false when comparing with null or incompatible types.
    ///     Verifies type safety and null handling in the Equals implementation.
    /// </summary>
    [TestMethod]
    public void Equals_WithNullOrIncompatibleType_ReturnsFalse()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var assetAmount = new AssetAmount(Asset.CreateNonNativeAsset("TEST", issuer.AccountId), "100");

        // Act & Assert
        Assert.IsFalse(assetAmount.Equals(null!));
        Assert.IsFalse(assetAmount.Equals("not an AssetAmount"));
        Assert.IsFalse(assetAmount.Equals(new object()));
    }
}