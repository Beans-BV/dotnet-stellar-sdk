using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class SetOptionsOperationResponseTest
{
    [TestMethod]
    public void TestDeserializeSetOptionsOperation()
    {
        var jsonPath = Utils.GetTestDataPath("setOptions.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertSetOptionsData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeSetOptionsOperation()
    {
        var jsonPath = Utils.GetTestDataPath("setOptions.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertSetOptionsData(back);
    }

    private static void AssertSetOptionsData(OperationResponse instance)
    {
        Assert.IsTrue(instance is SetOptionsOperationResponse);
        var operation = (SetOptionsOperationResponse)instance;

        Assert.AreEqual("GD3ZYXVC7C3ECD5I4E5NGPBFJJSULJ6HJI2FBHGKYFV34DSIWB4YEKJZ", operation.SignerKey);
        Assert.AreEqual(1, operation.SignerWeight);
        Assert.AreEqual("stellar.org", operation.HomeDomain);
        Assert.AreEqual("GBYWSY4NPLLPTP22QYANGTT7PEHND64P4D4B6LFEUHGUZRVYJK2H4TBE", operation.InflationDestination);
        Assert.AreEqual(1, operation.LowThreshold);
        Assert.AreEqual(2, operation.MedThreshold);
        Assert.AreEqual(3, operation.HighThreshold);
        Assert.AreEqual(4, operation.MasterKeyWeight);
        Assert.IsNotNull(operation.SetFlags);
        Assert.IsNotNull(operation.ClearFlags);
        Assert.AreEqual("auth_required_flag", operation.SetFlags[0]);
        Assert.AreEqual("auth_revocable_flag", operation.ClearFlags[0]);
    }

    [TestMethod]
    public void TestDeserializeSetOptionsOperationWithNonEd25519Key()
    {
        var jsonPath = Utils.GetTestDataPath("setOptionsNonEd25519Key.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertSetOptionsOperationWithNonEd25519KeyData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeSetOptionsOperationWithNonEd25519Key()
    {
        var jsonPath = Utils.GetTestDataPath("setOptionsNonEd25519Key.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertSetOptionsOperationWithNonEd25519KeyData(back);
    }

    private static void AssertSetOptionsOperationWithNonEd25519KeyData(OperationResponse instance)
    {
        Assert.IsTrue(instance is SetOptionsOperationResponse);
        var operation = (SetOptionsOperationResponse)instance;

        Assert.AreEqual("TBGFYVCU76LJ7GZOCGR4X7DG2NV42JPG5CKRL42LA5FZOFI3U2WU7ZAL", operation.SignerKey);
    }
}