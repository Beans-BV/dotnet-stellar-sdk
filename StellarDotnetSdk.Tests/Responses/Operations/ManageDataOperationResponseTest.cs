using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

/// <summary>
/// Unit tests for <see cref="ManageDataOperationResponse"/> class.
/// </summary>
[TestClass]
public class ManageDataOperationResponseTest
{
    /// <summary>
    /// Verifies that ManageDataOperationResponse can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageDataOperationJson_ReturnsDeserializedOperation()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("manageData.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertManageDataData(instance);
    }

    /// <summary>
    /// Verifies that ManageDataOperationResponse can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithManageDataOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("manageData.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertManageDataData(back);
    }

    private static void AssertManageDataData(OperationResponse instance)
    {
        Assert.IsTrue(instance is ManageDataOperationResponse);
        var operation = (ManageDataOperationResponse)instance;

        Assert.AreEqual(14336188517191688L, operation.Id);
        Assert.AreEqual("CollateralValue", operation.Name);
        Assert.AreEqual("MjAwMA==", operation.Value);
    }

    /// <summary>
    /// Verifies that ManageDataOperationResponse with empty value can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageDataOperationValueEmptyJson_ReturnsDeserializedOperation()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("manageDataValueEmpty.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertManageDataValueEmptyData(instance);
    }

    /// <summary>
    /// Verifies that ManageDataOperationResponse with empty value can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithManageDataOperationValueEmpty_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("manageDataValueEmpty.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertManageDataValueEmptyData(back);
    }

    private static void AssertManageDataValueEmptyData(OperationResponse instance)
    {
        Assert.IsTrue(instance is ManageDataOperationResponse);
        var operation = (ManageDataOperationResponse)instance;

        Assert.AreEqual(null, operation.Value);
    }
}