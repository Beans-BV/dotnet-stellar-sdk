using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class BeginSponsoringFutureReservesOperationTest
{
    [TestMethod]
    public void TestSerializationBeginSponsoringFutureReservesOperation()
    {
        var jsonPath = Utils.GetTestDataPath("beginSponsoringFutureReserves.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertBeginSponsoringFutureReservesData(back);
    }

    private static void AssertBeginSponsoringFutureReservesData(OperationResponse instance)
    {
        Assert.IsTrue(instance is BeginSponsoringFutureReservesOperationResponse);
        var operation = (BeginSponsoringFutureReservesOperationResponse)instance;

        Assert.AreEqual(215542933753857, operation.Id);
        Assert.AreEqual("GAXHU2XHSMTZYAKFCVTULAYUL34BFPPLRVJYZMEOHP7IWPZJKSVY67RJ", operation.SponsoredId);
    }
}