using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class PathPaymentStrictSendOperationResponseTest
{
    [TestMethod]
    public void TestDeserializePathPaymentStrictSendOperation()
    {
        var jsonPath = Utils.GetTestDataPath("pathPaymentStrictSend.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertPathPaymentStrictSendData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializePathPaymentStrictSendOperation()
    {
        var jsonPath = Utils.GetTestDataPath("pathPaymentStrictSend.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertPathPaymentStrictSendData(back);
    }

    private static void AssertPathPaymentStrictSendData(OperationResponse instance)
    {
        Assert.IsTrue(instance is PathPaymentStrictSendOperationResponse);
        var operation = (PathPaymentStrictSendOperationResponse)instance;

        Assert.AreEqual("GCXVEEBWI4YMRK6AFJQSEUBYDQL4PZ24ECAPJE2ZIAPIQZLZIBAX3LIF", operation.From);
        Assert.AreEqual("GCXVEEBWI4YMRK6AFJQSEUBYDQL4PZ24ECAPJE2ZIAPIQZLZIBAX3LIF", operation.To);
        Assert.IsNull(operation.FromMuxed);
        Assert.IsNull(operation.FromMuxedId);
        Assert.IsNull(operation.ToMuxed);
        Assert.IsNull(operation.ToMuxedId);
        Assert.AreEqual("0.0859000", operation.Amount);
        Assert.AreEqual("1000.0000000", operation.SourceAmount);
        Assert.AreEqual("0.0859000", operation.DestinationMin);
        Assert.AreEqual(Asset.Create("native", "", ""), operation.DestinationAsset);
        Assert.AreEqual(Asset.CreateNonNativeAsset("KIN", "GBDEVU63Y6NTHJQQZIKVTC23NWLQVP3WJ2RI2OTSJTNYOIGICST6DUXR"),
            operation.SourceAsset);
    }

    [TestMethod]
    public void TestDeserializePathPaymentStrictSendOperationMuxed()
    {
        var jsonPath = Utils.GetTestDataPath("pathPaymentStrictSendMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertPathPaymentStrictSendMuxedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializePathPaymentStrictSendOperationMuxed()
    {
        var jsonPath = Utils.GetTestDataPath("pathPaymentStrictSendMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertPathPaymentStrictSendMuxedData(back);
    }

    private static void AssertPathPaymentStrictSendMuxedData(OperationResponse instance)
    {
        Assert.IsTrue(instance is PathPaymentStrictSendOperationResponse);
        var operation = (PathPaymentStrictSendOperationResponse)instance;

        Assert.AreEqual(0, operation.Path.Length);
        Assert.AreEqual("GB7BTYMGED4DATO5U2BMPWKYABQQ3QBOQZK5T46N5CSCVPI2G3PVVYMB", operation.From);
        Assert.AreEqual("MB7BTYMGED4DATO5U2BMPWKYABQQ3QBOQZK5T46N5CSCVPI2G3PVUAAAAAAAAABRN3NWG", operation.FromMuxed);
        Assert.AreEqual(12654UL, operation.FromMuxedId);
        Assert.AreEqual("GBZG3SMBL6FPLYYNQP6DMVZHDHCDIR4J4GYRGKE5BYRVLBYN364RBL5S", operation.To);
        Assert.AreEqual("MBZG3SMBL6FPLYYNQP6DMVZHDHCDIR4J4GYRGKE5BYRVLBYN364RAAAAAAAAAAICXIM64", operation.ToMuxed);
        Assert.AreEqual(66234UL, operation.ToMuxedId);
        Assert.AreEqual("6922.2222222", operation.Amount);
        Assert.AreEqual("623.0000000", operation.SourceAmount);
        Assert.AreEqual("72.0000000", operation.DestinationMin);
        Assert.AreEqual("native", operation.SourceAssetType);
        Assert.IsNull(operation.SourceAssetCode);
        Assert.IsNull(operation.SourceAssetIssuer);
        Assert.AreEqual(new AssetTypeNative(), operation.SourceAsset);
        Assert.AreEqual("credit_alphanum4", operation.AssetType);
        Assert.AreEqual("TEST", operation.AssetCode);
        Assert.AreEqual("GB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST", operation.AssetIssuer);
        Assert.AreEqual(Asset.CreateNonNativeAsset("TEST", "GB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST"),
            operation.DestinationAsset);
    }
}