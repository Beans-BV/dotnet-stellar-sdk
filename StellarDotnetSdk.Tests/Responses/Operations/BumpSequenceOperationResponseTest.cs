using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class BumpSequenceOperationResponseTest
{
    [TestMethod]
    public void TestDeserializeBumpSequenceOperation()
    {
        var jsonPath = Utils.GetTestDataPath("bumpSequence.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertBumpSequenceData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeBumpSequenceOperation()
    {
        var jsonPath = Utils.GetTestDataPath("bumpSequence.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertBumpSequenceData(back);
    }

    private static void AssertBumpSequenceData(OperationResponse instance)
    {
        Assert.IsTrue(instance is BumpSequenceOperationResponse);
        var operation = (BumpSequenceOperationResponse)instance;

        Assert.AreEqual(12884914177L, operation.Id);
        Assert.AreEqual("79473726952833048", operation.BumpTo);
    }
}