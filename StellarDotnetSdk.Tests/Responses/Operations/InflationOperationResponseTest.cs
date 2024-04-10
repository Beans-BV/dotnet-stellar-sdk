using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Responses;
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
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        Assert.IsNotNull(instance);
        AssertInflationData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeInflationOperation()
    {
        var jsonPath = Utils.GetTestDataPath("inflation.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
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