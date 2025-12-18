using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

/// <summary>
/// Unit tests for liquidity pool operation responses.
/// </summary>
[TestClass]
public class LiquidityPoolOperationResponseTest
{
    /// <summary>
    /// Verifies that LiquidityPoolDepositOperationResponse can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithLiquidityPoolDepositOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolDepositOperationResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        Assert.IsTrue(back is LiquidityPoolDepositOperationResponse);
        var response = (LiquidityPoolDepositOperationResponse)back;
        Assert.AreEqual(new LiquidityPoolId("b26c0d6545349ad7f44ba758b7c705459537201583f2e524635be04aff84bc69"),
            response.LiquidityPoolId);
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

    /// <summary>
    /// Verifies that LiquidityPoolWithdrawOperationResponse can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithLiquidityPoolWithdrawOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolWithdrawOperationResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance);
        Assert.IsNotNull(serialized);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        Assert.IsTrue(back is LiquidityPoolWithdrawOperationResponse);
        var response = (LiquidityPoolWithdrawOperationResponse)back;
        Assert.AreEqual(new LiquidityPoolId("b26c0d6545349ad7f44ba758b7c705459537201583f2e524635be04aff84bc69"),
            response.LiquidityPoolId);
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