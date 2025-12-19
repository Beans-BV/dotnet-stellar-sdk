using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

/// <summary>
///     Unit tests for <see cref="ChangeTrustOperationResponse" /> class.
/// </summary>
[TestClass]
public class ChangeTrustOperationResponseTest
{
    /// <summary>
    ///     Verifies that ChangeTrustOperationResponse can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithChangeTrustOperationJson_ReturnsDeserializedOperation()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("changeTrust.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertChangeTrustData(instance);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperationResponse can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithChangeTrustOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("changeTrust.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertChangeTrustData(back);
    }

    private static void AssertChangeTrustData(OperationResponse instance)
    {
        Assert.IsTrue(instance is ChangeTrustOperationResponse);
        var operation = (ChangeTrustOperationResponse)instance;

        Assert.AreEqual("GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM", operation.Trustee);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.Trustor);
        Assert.IsNull(operation.TrustorMuxed);
        Assert.IsNull(operation.TrustorMuxedId);
        Assert.IsNull(operation.LiquidityPoolId);
        Assert.AreEqual("922337203685.4775807", operation.Limit);
        Assert.AreEqual(Asset.CreateNonNativeAsset("EUR", "GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM"),
            operation.Asset);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperationResponse with muxed account can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithChangeTrustOperationMuxedJson_ReturnsDeserializedOperation()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("changeTrustMuxed.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertChangeTrustDataMuxed(instance);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperationResponse with muxed account can be serialized and deserialized correctly
    ///     (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithChangeTrustOperationMuxed_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("changeTrustMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertChangeTrustDataMuxed(back);
    }

    private static void AssertChangeTrustDataMuxed(OperationResponse instance)
    {
        Assert.IsTrue(instance is ChangeTrustOperationResponse);
        var operation = (ChangeTrustOperationResponse)instance;

        Assert.AreEqual("GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM", operation.Trustee);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.Trustor);
        Assert.AreEqual("MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24",
            operation.TrustorMuxed);
        Assert.AreEqual(5123456789UL, operation.TrustorMuxedId);
        Assert.IsNull(operation.LiquidityPoolId);
        Assert.AreEqual("922337203685.4775807", operation.Limit);
        Assert.AreEqual(Asset.CreateNonNativeAsset("EUR", "GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM"),
            operation.Asset);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperationResponse with liquidity pool shares can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithChangeTrustOperationLiquidityPoolSharesJson_ReturnsDeserializedOperation()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("changeTrustLiquidityPoolShares.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertChangeTrustDataLiquidityPoolShares(instance);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperationResponse with liquidity pool shares can be serialized and deserialized correctly
    ///     (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithChangeTrustOperationLiquidityPoolShares_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("changeTrustLiquidityPoolShares.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertChangeTrustDataLiquidityPoolShares(back);
    }

    private static void AssertChangeTrustDataLiquidityPoolShares(OperationResponse instance)
    {
        Assert.IsTrue(instance is ChangeTrustOperationResponse);
        var operation = (ChangeTrustOperationResponse)instance;

        Assert.IsNull(operation.Trustee);
        Assert.AreEqual("liquidity_pool_shares", operation.AssetType);
        Assert.AreEqual("922337203685.4775807", operation.Limit);
        Assert.AreEqual("3cdf19b3d5d41f753e0f33ebf039f2733851732ab8fe679dcc5d6adafb4700e3", operation.LiquidityPoolId);
        Assert.AreEqual("GB7BTYMGED4DATO5U2BMPWKYABQQ3QBOQZK5T46N5CSCVPI2G3PVVYMB", operation.Trustor);
        Assert.IsNull(operation.TrustorMuxed);
        Assert.IsNull(operation.TrustorMuxedId);
        Assert.IsNull(operation.AssetCode);
        Assert.IsNull(operation.AssetIssuer);
        Assert.IsNull(operation.Asset);
    }
}