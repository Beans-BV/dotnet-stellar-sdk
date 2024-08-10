using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class PathPaymentStrictReceiveOperationResponseTest
{
    [TestMethod]
    public void TestDeserializePathPaymentStrictReceiveOperation()
    {
        var jsonPath = Utils.GetTestDataPath("pathPaymentStrictReceive.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        Assert.IsNotNull(instance);
        AssertPathPaymentStrictReceiveData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializePathPaymentStrictReceiveOperation()
    {
        var jsonPath = Utils.GetTestDataPath("pathPaymentStrictReceive.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertPathPaymentStrictReceiveData(back);
    }

    private static void AssertPathPaymentStrictReceiveData(OperationResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is PathPaymentStrictReceiveOperationResponse);
        var operation = (PathPaymentStrictReceiveOperationResponse)instance;

        var operationTest = new PathPaymentStrictReceiveOperationResponse
        {
            From = "GCXKG6RN4ONIEPCMNFB732A436Z5PNDSRLGWK7GBLCMQLIFO4S7EYWVU",
            To = "GA5WBPYA5Y4WAEHXWR2UKO2UO4BUGHUQ74EUPKON2QHV4WRHOIRNKKH2",
            AssetCode = "EUR",
            AssetIssuer = "GCQPYGH4K57XBDENKKX55KDTWOTK5WDWRQOH2LHEDX3EKVIQRLMESGBG",
            AssetType = "credit_alphanum4",
            Amount = "10.0",
            SourceAssetCode = "USD",
            SourceAssetIssuer = "GC23QF2HUE52AMXUFUH3AYJAXXGXXV2VHXYYR6EYXETPKDXZSAW67XO4",
            SourceAssetType = "credit_alphanum4",
            SourceMax = "10.0",
            SourceAmount = "10.0",
            Path = new Asset[] { },
        };

        Assert.AreEqual(operation.From, operationTest.From);
        Assert.AreEqual(operation.To, operationTest.To);
        Assert.AreEqual(operation.Amount, operationTest.Amount);
        Assert.AreEqual(operation.SourceMax, operationTest.SourceMax);
        Assert.AreEqual(operation.SourceAmount, operationTest.SourceAmount);
        Assert.AreEqual(operation.DestinationAsset,
            Asset.CreateNonNativeAsset(operationTest.AssetCode, operation.AssetIssuer));
        Assert.AreEqual(operation.SourceAsset,
            Asset.CreateNonNativeAsset(operationTest.SourceAssetCode, operation.SourceAssetIssuer));
    }
}