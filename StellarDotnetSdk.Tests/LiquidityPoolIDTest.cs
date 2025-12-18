using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.LiquidityPool;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests;

/// <summary>
/// Unit tests for <see cref="LiquidityPoolId"/> class.
/// </summary>
[TestClass]
public class LiquidityPoolIdTest
{
    /// <summary>
    /// Verifies that LiquidityPoolId constructor creates pool ID with correct string representation.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidAssets_CreatesPoolIdWithCorrectString()
    {
        // Arrange
        var assetA = Asset.Create("native");
        var assetB = Asset.CreateNonNativeAsset("ABC", "GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3");

        // Act
        var liquidityPoolId = new LiquidityPoolId(
            XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, assetA, assetB,
            LiquidityPoolParameters.Fee);

        // Assert
        Assert.AreEqual("cc22414997d7e3d9a9ac3b1d65ca9cc3e5f35ce33e0bd6a885648b11aaa3b72d", liquidityPoolId.ToString());
    }

    /// <summary>
    /// Verifies that LiquidityPoolId constructor throws ArgumentException when assets are not in lexicographic order.
    /// </summary>
    [TestMethod]
    public void Constructor_WithAssetsNotInLexicographicOrder_ThrowsArgumentException()
    {
        // Arrange
        var assetA = Asset.Create("native");
        var assetB = Asset.CreateNonNativeAsset("ABC", "GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3");

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(
            () => new LiquidityPoolId(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetB, assetA,
                LiquidityPoolParameters.Fee), "Asset A must be < Asset B (Lexicographic Order)");
    }

    /// <summary>
    /// Verifies that LiquidityPoolId Equals returns true for same pool ID instance.
    /// </summary>
    [TestMethod]
    public void Equals_WithSameInstance_ReturnsTrue()
    {
        // Arrange
        var assetA = Asset.Create("native");
        var assetB = Asset.CreateNonNativeAsset("ABC", "GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3");

        var pool1 = new LiquidityPoolId(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
            assetA,
            assetB, LiquidityPoolParameters.Fee);

        // Act & Assert
        Assert.AreEqual(pool1, pool1);
    }

    /// <summary>
    /// Verifies that LiquidityPoolId Equals returns false for different pool IDs.
    /// </summary>
    [TestMethod]
    public void Equals_WithDifferentPoolIds_ReturnsFalse()
    {
        // Arrange
        var assetA = Asset.Create("native");
        var assetB = Asset.CreateNonNativeAsset("ABC", "GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3");
        var assetC = Asset.CreateNonNativeAsset("ABCD", "GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3");

        var pool1 = new LiquidityPoolId(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
            assetA,
            assetB, LiquidityPoolParameters.Fee);
        var pool2 = new LiquidityPoolId(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
            assetB,
            assetC, LiquidityPoolParameters.Fee);

        // Act & Assert
        Assert.AreNotEqual(pool1, pool2);
    }
}