using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class ClawbackOperationResponseTest
{
    //Clawback
    [TestMethod]
    public void TestSerializeClawback()
    {
        var jsonPath = Utils.GetTestDataPath("clawback.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertClawbackData(back);
    }

    private static void AssertClawbackData(OperationResponse instance)
    {
        Assert.IsTrue(instance is ClawbackOperationResponse);
        var operation = (ClawbackOperationResponse)instance;

        Assert.AreEqual(3602979345141761, operation.Id);
        Assert.AreEqual(operation.Amount, "1000");
        Assert.AreEqual(operation.AssetCode, "EUR");
        Assert.AreEqual(operation.AssetIssuer, "GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM");
        Assert.AreEqual(operation.AssetType, "credit_alphanum4");
        Assert.AreEqual(operation.From, "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2");
        Assert.IsNull(operation.FromMuxed);
        Assert.IsNull(operation.FromMuxedID);
        Assert.AreEqual(operation.Asset.ToQueryParameterEncodedString(),
            "EUR:GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM");
    }

    //Clawback (Muxed)
    [TestMethod]
    public void TestSerializeClawbackMuxed()
    {
        var jsonPath = Utils.GetTestDataPath("clawbackMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertClawbackDataMuxed(back);
    }

    private static void AssertClawbackDataMuxed(OperationResponse instance)
    {
        Assert.IsTrue(instance is ClawbackOperationResponse);
        var operation = (ClawbackOperationResponse)instance;

        Assert.AreEqual(3602979345141761, operation.Id);
        Assert.AreEqual(operation.Amount, "1000");
        Assert.AreEqual(operation.AssetCode, "EUR");
        Assert.AreEqual(operation.AssetIssuer, "GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM");
        Assert.AreEqual(operation.AssetType, "credit_alphanum4");
        Assert.AreEqual(operation.From, "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2");
        Assert.AreEqual(operation.FromMuxed, "MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24");
        Assert.AreEqual(operation.FromMuxedID, 5123456789UL);
        Assert.AreEqual(operation.Asset.ToQueryParameterEncodedString(),
            "EUR:GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM");
    }
}