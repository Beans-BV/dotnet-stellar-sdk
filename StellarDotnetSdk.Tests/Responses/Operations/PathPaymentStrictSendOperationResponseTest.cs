using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;
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
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        Assert.IsNotNull(instance);
        AssertPathPaymentStrictSendData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializePathPaymentStrictSendOperation()
    {
        var jsonPath = Utils.GetTestDataPath("pathPaymentStrictSend.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertPathPaymentStrictSendData(back);
    }

    private static void AssertPathPaymentStrictSendData(OperationResponse instance)
    {
        Assert.IsTrue(instance is PathPaymentStrictSendOperationResponse);
        var operation = (PathPaymentStrictSendOperationResponse)instance;

        var operationTest = new PathPaymentStrictSendOperationResponse
        {
            From = "GCXVEEBWI4YMRK6AFJQSEUBYDQL4PZ24ECAPJE2ZIAPIQZLZIBAX3LIF",
            To = "GCXVEEBWI4YMRK6AFJQSEUBYDQL4PZ24ECAPJE2ZIAPIQZLZIBAX3LIF",
            AssetType = "native",
            AssetCode = "",
            AssetIssuer = "",
            Amount = "0.0859000",
            SourceAssetType = "credit_alphanum4",
            SourceAssetCode = "KIN",
            SourceAssetIssuer = "GBDEVU63Y6NTHJQQZIKVTC23NWLQVP3WJ2RI2OTSJTNYOIGICST6DUXR",
            SourceAmount = "1000.0000000",
            DestinationMin = "0.0859000",
            Path = new Asset[] { }
        };

        Assert.AreEqual(operation.From, operationTest.From);
        Assert.AreEqual(operation.To, operationTest.To);
        Assert.AreEqual(operation.Amount, operationTest.Amount);
        Assert.AreEqual(operation.SourceAmount, operationTest.SourceAmount);
        Assert.AreEqual(operation.DestinationMin, operationTest.DestinationMin);
        Assert.AreEqual(operation.DestinationAsset, Asset.Create(operationTest.AssetType, "", ""));
        Assert.AreEqual(operation.SourceAsset,
            Asset.CreateNonNativeAsset(operationTest.SourceAssetCode, operationTest.SourceAssetIssuer));
    }
}