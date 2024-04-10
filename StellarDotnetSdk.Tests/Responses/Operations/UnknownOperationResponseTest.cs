using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class UnknownOperationResponseTest
{
    [TestMethod]
    public void TestDeserializeUnknownOperation()
    {
        var jsonPath = Utils.GetTestDataPath("unknownOperation.json");
        var json = File.ReadAllText(jsonPath);
        Assert.ThrowsException<JsonSerializationException>(() =>
            JsonSingleton.GetInstance<OperationResponse>(json));
    }
}