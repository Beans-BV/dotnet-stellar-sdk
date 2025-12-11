using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class SetTrustlineFlagsOperationResponseTest
{
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

        Assert.AreEqual("credit_alphanum4", operation.AssetType);
        Assert.AreEqual("EUR", operation.AssetCode);
        Assert.AreEqual("GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM", operation.AssetIssuer);
        Assert.AreEqual("GTRUSTORYHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM", operation.Trustor);
        Assert.AreEqual("authorized", operation.SetFlags[0]);
        Assert.AreEqual("authorized_to_maintain_liabilities", operation.ClearFlags[0]);
    }
}