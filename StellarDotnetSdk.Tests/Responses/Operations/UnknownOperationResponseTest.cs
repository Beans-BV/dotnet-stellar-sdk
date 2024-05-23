using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
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
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions));
    }
}