using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

/// <summary>
///     Unit tests for <see cref="AccountMergeOperationResponse" /> class.
/// </summary>
[TestClass]
public class AccountMergeOperationResponseTest
{
    /// <summary>
    ///     Verifies that AccountMergeOperationResponse can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAccountMergeOperationJson_ReturnsDeserializedOperation()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("accountMerge.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertAccountMergeData(instance);
    }

    /// <summary>
    ///     Verifies that AccountMergeOperationResponse can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithAccountMergeOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("accountMerge.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertAccountMergeData(back);
    }

    private static void AssertAccountMergeData(OperationResponse instance)
    {
        Assert.IsTrue(instance is AccountMergeOperationResponse);
        var operation = (AccountMergeOperationResponse)instance;

        Assert.AreEqual("GD6GKRABNDVYDETEZJQEPS7IBQMERCN44R5RCI4LJNX6BMYQM2KPGGZ2", operation.Account);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.Into);
        Assert.IsNull(operation.AccountMuxed);
        Assert.IsNull(operation.AccountMuxedId);
        Assert.IsNull(operation.IntoMuxed);
        Assert.IsNull(operation.IntoMuxedID);
    }

    /// <summary>
    ///     Verifies that AccountMergeOperationResponse with muxed accounts can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAccountMergeOperationMuxedJson_ReturnsDeserializedOperation()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("accountMergeMuxed.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertAccountMergeDataMuxed(instance);
    }

    /// <summary>
    ///     Verifies that AccountMergeOperationResponse with muxed accounts can be serialized and deserialized correctly
    ///     (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithAccountMergeOperationMuxed_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("accountMergeMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertAccountMergeDataMuxed(back);
    }

    private static void AssertAccountMergeDataMuxed(OperationResponse instance)
    {
        Assert.IsTrue(instance is AccountMergeOperationResponse);
        var operation = (AccountMergeOperationResponse)instance;

        Assert.AreEqual("GDI53A4VSMMYMVVTLO3X4SMZXWIPWN3ETEKTFVPOYO67A5FPLLK4T3YR", operation.Account);
        Assert.AreEqual("MDI53A4VSMMYMVVTLO3X4SMZXWIPWN3ETEKTFVPOYO67A5FPLLK4SAAAAAAAAAJKERIZA",
            operation.AccountMuxed);
        Assert.AreEqual(76324UL, operation.AccountMuxedId);
        Assert.AreEqual("GBZG3SMBL6FPLYYNQP6DMVZHDHCDIR4J4GYRGKE5BYRVLBYN364RBL5S", operation.Into);
        Assert.AreEqual("MBZG3SMBL6FPLYYNQP6DMVZHDHCDIR4J4GYRGKE5BYRVLBYN364RAAAAAAAAAAICXIM64", operation.IntoMuxed);
        Assert.AreEqual(66234UL, operation.IntoMuxedID);
    }
}