using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using stellar_dotnet_sdk;
using stellar_dotnet_sdk.responses;
using stellar_dotnet_sdk.responses.operations;

namespace stellar_dotnet_sdk_test.responses.operations;

[TestClass]
public class LiquidityPoolDepositOperationResponseTest
{
    //Allow Trust
    [TestMethod]
    public void TestDeserialize()
    {
        var json = File.ReadAllText(Path.Combine("responses/operations/LiquidityPoolDepositOperationResponse",
            "Data.json"));
        var instance =
            (LiquidityPoolDepositOperationResponse)JsonSingleton
                .GetInstance<OperationResponse>(json);

        AssertData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserialize()
    {
        var json = File.ReadAllText(Path.Combine("responses/operations/LiquidityPoolDepositOperationResponse",
            "Data.json"));
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertData((LiquidityPoolDepositOperationResponse)back);
    }

    public void AssertData(LiquidityPoolDepositOperationResponse instance)
    {
        Assert.AreEqual(new LiquidityPoolID("b26c0d6545349ad7f44ba758b7c705459537201583f2e524635be04aff84bc69"),
            instance.LiquidityPoolID);
        Assert.AreEqual("1508315204960257", instance.PagingToken);

        Assert.AreEqual("1.0000000", instance.MinPrice);
        Assert.AreEqual("100000000.0000000", instance.MaxPrice);

        Assert.AreEqual("1000.0000000", instance.ReservesMax[0].Amount);
        Assert.AreEqual("native", instance.ReservesMax[0].Asset.CanonicalName());

        Assert.AreEqual("1.0000000", instance.ReservesMax[1].Amount);
        Assert.AreEqual("NOODLE:GC2J4TNJKAKMJLTVAKVMA62CKRNC3YZDEK4WZUI4XZUM4DGPRX7ZMW7S",
            instance.ReservesMax[1].Asset.CanonicalName());

        Assert.AreEqual("0.0000000", instance.ReservesDeposited[0].Amount);
        Assert.AreEqual("native", instance.ReservesDeposited[0].Asset.CanonicalName());
        Assert.AreEqual("0.0000000", instance.ReservesDeposited[1].Amount);
        Assert.AreEqual("NOODLE:GC2J4TNJKAKMJLTVAKVMA62CKRNC3YZDEK4WZUI4XZUM4DGPRX7ZMW7S",
            instance.ReservesDeposited[1].Asset.CanonicalName());

        Assert.AreEqual("0.0000000", instance.SharesReceived);
    }
}