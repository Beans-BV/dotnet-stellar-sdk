using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class ChangeTrustOperationResponseTest
{
    [TestMethod]
    public void TestDeserializeChangeTrustOperation()
    {
        var jsonPath = Utils.GetTestDataPath("changeTrust.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertChangeTrustData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeChangeTrustOperation()
    {
        var jsonPath = Utils.GetTestDataPath("changeTrust.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertChangeTrustData(back);
    }

    private static void AssertChangeTrustData(OperationResponse instance)
    {
        Assert.IsTrue(instance is ChangeTrustOperationResponse);
        var operation = (ChangeTrustOperationResponse)instance;

        Assert.AreEqual("GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM", operation.Trustee);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.Trustor);
        Assert.IsNull(operation.TrustorMuxed);
        Assert.IsNull(operation.TrustorMuxedId);
        Assert.IsNull(operation.LiquidityPoolId);
        Assert.AreEqual("922337203685.4775807", operation.Limit);
        Assert.AreEqual(Asset.CreateNonNativeAsset("EUR", "GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM"),
            operation.Asset);
    }

    [TestMethod]
    public void TestDeserializeChangeTrustOperationMuxed()
    {
        var jsonPath = Utils.GetTestDataPath("changeTrustMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertChangeTrustDataMuxed(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeChangeTrustOperationMuxed()
    {
        var jsonPath = Utils.GetTestDataPath("changeTrustMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertChangeTrustDataMuxed(back);
    }

    private static void AssertChangeTrustDataMuxed(OperationResponse instance)
    {
        Assert.IsTrue(instance is ChangeTrustOperationResponse);
        var operation = (ChangeTrustOperationResponse)instance;

        Assert.AreEqual("GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM", operation.Trustee);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.Trustor);
        Assert.AreEqual("MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24",
            operation.TrustorMuxed);
        Assert.AreEqual(5123456789UL, operation.TrustorMuxedId);
        Assert.IsNull(operation.LiquidityPoolId);
        Assert.AreEqual("922337203685.4775807", operation.Limit);
        Assert.AreEqual(Asset.CreateNonNativeAsset("EUR", "GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM"),
            operation.Asset);
    }

    [TestMethod]
    public void TestDeserializeChangeTrustOperationLiquidityPoolShares()
    {
        var jsonPath = Utils.GetTestDataPath("changeTrustLiquidityPoolShares.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertChangeTrustDataLiquidityPoolShares(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeChangeTrustOperationLiquidityPoolShares()
    {
        var jsonPath = Utils.GetTestDataPath("changeTrustLiquidityPoolShares.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertChangeTrustDataLiquidityPoolShares(back);
    }

    private static void AssertChangeTrustDataLiquidityPoolShares(OperationResponse instance)
    {
        Assert.IsTrue(instance is ChangeTrustOperationResponse);
        var operation = (ChangeTrustOperationResponse)instance;

        Assert.IsNull(operation.Trustee);
        Assert.AreEqual("liquidity_pool_shares", operation.AssetType);
        Assert.AreEqual("922337203685.4775807", operation.Limit);
        Assert.AreEqual("3cdf19b3d5d41f753e0f33ebf039f2733851732ab8fe679dcc5d6adafb4700e3", operation.LiquidityPoolId);
        Assert.AreEqual("GB7BTYMGED4DATO5U2BMPWKYABQQ3QBOQZK5T46N5CSCVPI2G3PVVYMB", operation.Trustor);
        Assert.IsNull(operation.TrustorMuxed);
        Assert.IsNull(operation.TrustorMuxedId);
        Assert.IsNull(operation.AssetCode);
        Assert.IsNull(operation.AssetIssuer);
        Assert.IsNull(operation.Asset);
    }
}