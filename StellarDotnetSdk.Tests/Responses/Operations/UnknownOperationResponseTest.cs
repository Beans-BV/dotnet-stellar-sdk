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
        var json = File.ReadAllText(Path.Combine("testdata/operations/unknownOperation", "unknownOperation.json"));
        Assert.ThrowsException<JsonSerializationException>(() =>
            JsonSingleton.GetInstance<OperationResponse>(json));
    }
}