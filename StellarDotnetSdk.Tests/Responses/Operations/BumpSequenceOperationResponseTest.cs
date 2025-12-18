using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

/// <summary>
///     Unit tests for <see cref="BumpSequenceOperationResponse" /> class.
/// </summary>
[TestClass]
public class BumpSequenceOperationResponseTest
{
    /// <summary>
    ///     Verifies that BumpSequenceOperationResponse can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithBumpSequenceOperationJson_ReturnsDeserializedOperation()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("bumpSequence.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertBumpSequenceData(instance);
    }

    /// <summary>
    ///     Verifies that BumpSequenceOperationResponse can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithBumpSequenceOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("bumpSequence.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertBumpSequenceData(back);
    }

    private static void AssertBumpSequenceData(OperationResponse instance)
    {
        Assert.IsTrue(instance is BumpSequenceOperationResponse);
        var operation = (BumpSequenceOperationResponse)instance;

        Assert.AreEqual(8627811908587521L, operation.Id);
        Assert.AreEqual(100L, operation.BumpTo);
    }
}