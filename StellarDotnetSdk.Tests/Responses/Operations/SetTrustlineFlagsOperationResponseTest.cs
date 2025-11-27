using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class SetTrustlineFlagsOperationResponseTest
{
    //Set Trustline Flags
    [TestMethod]
    public void TestSetTrustlineFlags()
    {
        var jsonPath = Utils.GetTestDataPath("setTrustlineFlags.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertTestSetTrustlineFlagsData(back);
    }

    private static void AssertTestSetTrustlineFlagsData(OperationResponse instance)
    {
        Assert.IsTrue(instance is SetTrustlineFlagsOperationResponse);
        var operation = (SetTrustlineFlagsOperationResponse)instance;
        var operation2 = new SetTrustlineFlagsOperationResponse
        {
            AssetType = "credit_alphanum4",
            AssetCode = "EUR",
            AssetIssuer = "GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM",
            Trustor = "GTRUSTORYHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM",
            ClearFlags = ["authorized_to_maintain_liabilities"],
            SetFlags = ["authorized"],
        };
        Assert.AreEqual(operation.AssetType, operation2.AssetType);
        Assert.AreEqual(operation.AssetCode, operation2.AssetCode);
        Assert.AreEqual(operation.AssetIssuer, operation2.AssetIssuer);
        Assert.AreEqual(operation.Trustor, operation2.Trustor);
        Assert.AreEqual(operation.SetFlags[0], operation2.SetFlags[0]);
        Assert.AreEqual(operation.ClearFlags[0], operation2.ClearFlags[0]);
        Assert.AreEqual(operation.Asset, operation2.Asset);
    }
}