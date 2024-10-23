using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class LiquidityPoolOperationResponseTest
{
    [TestMethod]
    public void TestPoolDeposit()
    {
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolDepositOperationResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        Assert.IsTrue(back is LiquidityPoolDepositOperationResponse);
        Assert.IsNotNull(instance);
        var response = (LiquidityPoolDepositOperationResponse)instance;
        Assert.AreEqual(new LiquidityPoolID("b26c0d6545349ad7f44ba758b7c705459537201583f2e524635be04aff84bc69"),
            response.LiquidityPoolID);
        Assert.AreEqual("1508315204960257", response.PagingToken);

        Assert.AreEqual("1.0000000", response.MinPrice);
        Assert.AreEqual("100000000.0000000", response.MaxPrice);

        Assert.AreEqual("1000.0000000", response.ReservesMax[0].Amount);
        Assert.AreEqual("native", response.ReservesMax[0].Asset.CanonicalName());

        Assert.AreEqual("1.0000000", response.ReservesMax[1].Amount);
        Assert.AreEqual("NOODLE:GC2J4TNJKAKMJLTVAKVMA62CKRNC3YZDEK4WZUI4XZUM4DGPRX7ZMW7S",
            response.ReservesMax[1].Asset.CanonicalName());

        Assert.AreEqual("0.0000000", response.ReservesDeposited[0].Amount);
        Assert.AreEqual("native", response.ReservesDeposited[0].Asset.CanonicalName());
        Assert.AreEqual("0.0000000", response.ReservesDeposited[1].Amount);
        Assert.AreEqual("NOODLE:GC2J4TNJKAKMJLTVAKVMA62CKRNC3YZDEK4WZUI4XZUM4DGPRX7ZMW7S",
            response.ReservesDeposited[1].Asset.CanonicalName());

        Assert.AreEqual("0.0000000", response.SharesReceived);
    }

    [TestMethod]
    public void TestPoolWithdraw()
    {
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolWithdrawOperationResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        Assert.IsNotNull(serialized);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        Assert.IsTrue(back is LiquidityPoolWithdrawOperationResponse);
        Assert.IsNotNull(instance);
        var response = (LiquidityPoolWithdrawOperationResponse)instance;
        Assert.AreEqual(new LiquidityPoolID("b26c0d6545349ad7f44ba758b7c705459537201583f2e524635be04aff84bc69"),
            response.LiquidityPoolID);
        Assert.AreEqual("1508641622462465", response.PagingToken);

        Assert.AreEqual("0.0000000", response.ReservesMin[0].Amount);
        Assert.AreEqual("native", response.ReservesMin[0].Asset.CanonicalName());

        Assert.AreEqual("0.0000000", response.ReservesMin[1].Amount);
        Assert.AreEqual("USDC:GAKMOAANQHJKF5735OYVSQZL6KC3VMFL4LP4ZYY2LWK256TSUG45IEFB",
            response.ReservesMin[1].Asset.CanonicalName());

        Assert.AreEqual("1000.0000000", response.Shares);

        Assert.AreEqual("1000.0000000", response.ReservesReceived[0].Amount);
        Assert.AreEqual("native", response.ReservesReceived[0].Asset.CanonicalName());

        Assert.AreEqual("1000.0000000", response.ReservesReceived[1].Amount);
        Assert.AreEqual("USDC:GAKMOAANQHJKF5735OYVSQZL6KC3VMFL4LP4ZYY2LWK256TSUG45IEFB",
            response.ReservesReceived[1].Asset.CanonicalName());
    }
}