using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class BeginSponsoringFutureReservesOperationTest
{
    //Begin Sponsoring Future Reserves
    [TestMethod]
    public void TestSerializationBeginSponsoringFutureReservesOperation()
    {
        var jsonPath = Utils.GetTestDataPath("beginSponsoringFutureReserves.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertBeginSponsoringFutureReservesData(back);
    }

    private static void AssertBeginSponsoringFutureReservesData(OperationResponse instance)
    {
        Assert.IsTrue(instance is BeginSponsoringFutureReservesOperationResponse);
        var operation = (BeginSponsoringFutureReservesOperationResponse)instance;

        Assert.AreEqual(215542933753857, operation.Id);
        Assert.AreEqual("GAXHU2XHSMTZYAKFCVTULAYUL34BFPPLRVJYZMEOHP7IWPZJKSVY67RJ", operation.SponsoredId);

        var back = new BeginSponsoringFutureReservesOperationResponse
        {
            SponsoredId = operation.SponsoredId
        };
        Assert.IsNotNull(back);
    }
}