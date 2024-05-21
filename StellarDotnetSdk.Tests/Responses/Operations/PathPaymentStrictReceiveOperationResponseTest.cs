using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
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
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertPathPaymentStrictReceiveData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializePathPaymentStrictReceiveOperation()
    {
        var jsonPath = Utils.GetTestDataPath("pathPaymentStrictReceive.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized);
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
            AssetType = "native",
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
        Assert.AreEqual(0, operation.DestinationAsset.CompareTo(operationTest.DestinationAsset));
        Assert.AreEqual(0, operation.SourceAsset.CompareTo(operationTest.SourceAsset));
    }
}