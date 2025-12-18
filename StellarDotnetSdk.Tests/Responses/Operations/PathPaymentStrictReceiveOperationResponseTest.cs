using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

/// <summary>
///     Unit tests for <see cref="PathPaymentStrictReceiveOperationResponse" /> class.
/// </summary>
[TestClass]
public class PathPaymentStrictReceiveOperationResponseTest
{
    /// <summary>
    ///     Verifies that PathPaymentStrictReceiveOperationResponse can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictReceiveOperationJson_ReturnsDeserializedOperation()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("pathPaymentStrictReceive.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertPathPaymentStrictReceiveData(instance);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictReceiveOperationResponse can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithPathPaymentStrictReceiveOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("pathPaymentStrictReceive.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertPathPaymentStrictReceiveData(back);
    }

    private static void AssertPathPaymentStrictReceiveData(OperationResponse instance)
    {
        Assert.IsTrue(instance is PathPaymentStrictReceiveOperationResponse);
        var operation = (PathPaymentStrictReceiveOperationResponse)instance;

        Assert.AreEqual("credit_alphanum4", operation.AssetType);
        Assert.AreEqual("TEST", operation.AssetCode);
        Assert.AreEqual("GB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST", operation.AssetIssuer);
        Assert.AreEqual(Asset.CreateNonNativeAsset("TEST", "GB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST"),
            operation.DestinationAsset);
        Assert.AreEqual(0, operation.Path.Count);
        Assert.AreEqual("GB7BTYMGED4DATO5U2BMPWKYABQQ3QBOQZK5T46N5CSCVPI2G3PVVYMB", operation.From);
        Assert.IsNull(operation.FromMuxed);
        Assert.IsNull(operation.FromMuxedId);
        Assert.IsNull(operation.ToMuxed);
        Assert.IsNull(operation.ToMuxedId);
        Assert.AreEqual("GBZG3SMBL6FPLYYNQP6DMVZHDHCDIR4J4GYRGKE5BYRVLBYN364RBL5S", operation.To);
        Assert.AreEqual("1.0000000", operation.Amount);
        Assert.AreEqual("15.0000000", operation.SourceMax);
        Assert.AreEqual("0.0900000", operation.SourceAmount);
        Assert.AreEqual("native", operation.SourceAssetType);
        Assert.IsNull(operation.SourceAssetCode);
        Assert.IsNull(operation.SourceAssetIssuer);
        Assert.AreEqual(new AssetTypeNative(), operation.SourceAsset);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictReceiveOperationResponse with muxed accounts can be deserialized from JSON
    ///     correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictReceiveOperationMuxedJson_ReturnsDeserializedOperation()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("pathPaymentStrictReceiveMuxed.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertPathPaymentStrictReceiveMuxedData(instance);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictReceiveOperationResponse with muxed accounts can be serialized and deserialized
    ///     correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithPathPaymentStrictReceiveOperationMuxed_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("pathPaymentStrictReceiveMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertPathPaymentStrictReceiveMuxedData(back);
    }

    private static void AssertPathPaymentStrictReceiveMuxedData(OperationResponse instance)
    {
        Assert.IsTrue(instance is PathPaymentStrictReceiveOperationResponse);
        var operation = (PathPaymentStrictReceiveOperationResponse)instance;

        Assert.AreEqual("credit_alphanum4", operation.AssetType);
        Assert.AreEqual("TEST", operation.AssetCode);
        Assert.AreEqual("GB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST", operation.AssetIssuer);
        Assert.AreEqual(Asset.CreateNonNativeAsset("TEST", "GB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST"),
            operation.DestinationAsset);
        Assert.AreEqual(0, operation.Path.Count);
        Assert.AreEqual("GB7BTYMGED4DATO5U2BMPWKYABQQ3QBOQZK5T46N5CSCVPI2G3PVVYMB", operation.From);
        Assert.AreEqual("MB7BTYMGED4DATO5U2BMPWKYABQQ3QBOQZK5T46N5CSCVPI2G3PVUAAAAAAAAABRN3NWG", operation.FromMuxed);
        Assert.AreEqual(12654UL, operation.FromMuxedId);
        Assert.AreEqual("GBZG3SMBL6FPLYYNQP6DMVZHDHCDIR4J4GYRGKE5BYRVLBYN364RBL5S", operation.To);
        Assert.AreEqual("MBZG3SMBL6FPLYYNQP6DMVZHDHCDIR4J4GYRGKE5BYRVLBYN364RAAAAAAAAAAICXIM64", operation.ToMuxed);
        Assert.AreEqual(66234UL, operation.ToMuxedId);
        Assert.AreEqual("1.0000000", operation.Amount);
        Assert.AreEqual("723.4100000", operation.SourceMax);
        Assert.AreEqual("0.0900000", operation.SourceAmount);
        Assert.AreEqual("native", operation.SourceAssetType);
        Assert.IsNull(operation.SourceAssetCode);
        Assert.IsNull(operation.SourceAssetIssuer);
        Assert.AreEqual(new AssetTypeNative(), operation.SourceAsset);
    }
}