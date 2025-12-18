using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

/// <summary>
///     Unit tests for <see cref="ClawbackOperationResponse" /> class.
/// </summary>
[TestClass]
public class ClawbackOperationResponseTest
{
    /// <summary>
    ///     Verifies that ClawbackOperationResponse can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithClawbackOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("clawback.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertClawbackData(back);
    }

    private static void AssertClawbackData(OperationResponse instance)
    {
        Assert.IsTrue(instance is ClawbackOperationResponse);
        var operation = (ClawbackOperationResponse)instance;

        Assert.AreEqual(3602979345141761, operation.Id);
        Assert.AreEqual("1000", operation.Amount);
        Assert.AreEqual("EUR", operation.AssetCode);
        Assert.AreEqual("GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM", operation.AssetIssuer);
        Assert.AreEqual("credit_alphanum4", operation.AssetType);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.From);
        Assert.IsNull(operation.FromMuxed);
        Assert.IsNull(operation.FromMuxedId);
        Assert.AreEqual("EUR:GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM",
            operation.Asset.ToQueryParameterEncodedString());
    }

    /// <summary>
    ///     Verifies that ClawbackOperationResponse with muxed account can be serialized and deserialized correctly
    ///     (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithClawbackOperationMuxed_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("clawbackMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertClawbackDataMuxed(back);
    }

    private static void AssertClawbackDataMuxed(OperationResponse instance)
    {
        Assert.IsTrue(instance is ClawbackOperationResponse);
        var operation = (ClawbackOperationResponse)instance;

        Assert.AreEqual(3602979345141761, operation.Id);
        Assert.AreEqual("1000", operation.Amount);
        Assert.AreEqual("EUR", operation.AssetCode);
        Assert.AreEqual("GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM", operation.AssetIssuer);
        Assert.AreEqual("credit_alphanum4", operation.AssetType);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.From);
        Assert.AreEqual("MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24", operation.FromMuxed);
        Assert.AreEqual(5123456789UL, operation.FromMuxedId);
        Assert.AreEqual("EUR:GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM",
            operation.Asset.ToQueryParameterEncodedString());
    }
}