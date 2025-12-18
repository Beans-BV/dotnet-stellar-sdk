using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

/// <summary>
///     Unit tests for <see cref="AllowTrustOperationResponse" /> class.
/// </summary>
[TestClass]
public class AllowTrustOperationResponseTest
{
    /// <summary>
    ///     Verifies that AllowTrustOperationResponse can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAllowTrustOperationJson_ReturnsDeserializedOperation()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("allowTrust.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertAllowTrustOperationData(instance);
    }

    /// <summary>
    ///     Verifies that AllowTrustOperationResponse can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithAllowTrustOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("allowTrust.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertAllowTrustOperationData(back);
    }

    private static void AssertAllowTrustOperationData(OperationResponse instance)
    {
        Assert.IsTrue(instance is AllowTrustOperationResponse);
        var operation = (AllowTrustOperationResponse)instance;

        Assert.AreEqual("GDZ55LVXECRTW4G36EZPTHI4XIYS5JUC33TUS22UOETVFVOQ77JXWY4F", operation.Trustor);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.Trustee);
        Assert.IsNull(operation.TrusteeMuxed);
        Assert.IsNull(operation.TrusteeMuxedId);
        Assert.AreEqual(true, operation.Authorize);
        Assert.AreEqual(Asset.CreateNonNativeAsset("EUR", "GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM"),
            operation.Asset);
    }

    /// <summary>
    ///     Verifies that AllowTrustOperationResponse with muxed account can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAllowTrustOperationMuxedJson_ReturnsDeserializedOperation()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("allowTrustMuxed.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertAllowTrustOperationMuxed(instance);
    }

    /// <summary>
    ///     Verifies that AllowTrustOperationResponse with muxed account can be serialized and deserialized correctly
    ///     (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithAllowTrustOperationMuxed_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("allowTrustMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertAllowTrustOperationMuxed(back);
    }

    private static void AssertAllowTrustOperationMuxed(OperationResponse instance)
    {
        Assert.IsTrue(instance is AllowTrustOperationResponse);
        var operation = (AllowTrustOperationResponse)instance;

        Assert.AreEqual("GDZ55LVXECRTW4G36EZPTHI4XIYS5JUC33TUS22UOETVFVOQ77JXWY4F", operation.Trustor);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.Trustee);
        Assert.AreEqual("MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24",
            operation.TrusteeMuxed);
        Assert.AreEqual(5123456789UL, operation.TrusteeMuxedId);
        Assert.AreEqual(true, operation.Authorize);
        Assert.AreEqual(Asset.CreateNonNativeAsset("EUR", "GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM"),
            operation.Asset);
    }
}