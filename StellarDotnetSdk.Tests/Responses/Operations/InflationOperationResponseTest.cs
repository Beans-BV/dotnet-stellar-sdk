using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class InflationOperationResponseTest
{
    [TestMethod]
    public void TestDeserializeInflationOperation()
    {
        var jsonPath = Utils.GetTestDataPath("inflation.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertInflationData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeInflationOperation()
    {
        var jsonPath = Utils.GetTestDataPath("inflation.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertInflationData(back);
    }

    private static void AssertInflationData(OperationResponse instance)
    {
        Assert.IsTrue(instance is InflationOperationResponse);
        var operation = (InflationOperationResponse)instance;

        Assert.AreEqual(operation.Id, 12884914177L);
    }
}