using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Responses;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class LiquidityPoolDeserializerTest
{
    [TestMethod]
    public void TestDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("liquidityPool.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<LiquidityPoolResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertTestData(instance);
    }


    [TestMethod]
    public void TestSerializeDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("liquidityPool.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<LiquidityPoolResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var parsed = JsonSerializer.Deserialize<LiquidityPoolResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(parsed);
        AssertTestData(parsed);
    }

    [TestMethod]
    public void TestReserveEquality()
    {
        var assetA = Asset.Create("PHP:GAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S");
        var assetB = Asset.Create("EURT:GAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S");
        var reserveA = new Reserve
        {
            Amount = "2000.0000000",
            Asset = assetA,
        };

        var reserveB = new Reserve
        {
            Amount = "2000.0000000",
            Asset = assetA,
        };
        var reserveC = new Reserve
        {
            Amount = "1000.0000005",
            Asset = assetA,
        };
        var reserveD = new Reserve
        {
            Amount = "2000.0000000",
            Asset = assetB,
        };

        Assert.AreEqual(reserveA, reserveB);
        Assert.AreNotEqual(reserveA, reserveC);
        Assert.AreNotEqual(reserveA, reserveD);
        Assert.AreNotEqual(reserveC, reserveD);
    }


    internal static void AssertTestData(LiquidityPoolResponse instance)
    {
        Assert.IsNotNull(instance);
        Assert.AreEqual(new LiquidityPoolId("93f526d8bc7b38ac6d746c2d0a4bebb548ea31e1574cb146a6e6898030d05144"),
            instance.Id);
        Assert.AreEqual("93f526d8bc7b38ac6d746c2d0a4bebb548ea31e1574cb146a6e6898030d05144", instance.PagingToken);
        Assert.AreEqual(30, instance.FeeBp);
        Assert.AreEqual(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, instance.Type);
        Assert.AreEqual("1", instance.TotalTrustlines);
        Assert.AreEqual("666.6666664", instance.TotalShares);

        Assert.AreEqual("310.0000000", instance.Reserves[0].Amount);
        Assert.AreEqual("native", instance.Reserves[0].Asset.CanonicalName());

        Assert.AreEqual("1435.2855840", instance.Reserves[1].Amount);
        Assert.AreEqual("TEST:GC6ZBHGJGGTPVLYALOKQNQSQUXHJUYDZ7VLMAPU2MERVTYMKVL2GTEST",
            instance.Reserves[1].Asset.CanonicalName());
        Assert.AreEqual(1694L, instance.LastModifiedLedger);
        Assert.AreEqual("2025-08-14T19:44:19Z", instance.LastModifiedTime);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/liquidity_pools/93f526d8bc7b38ac6d746c2d0a4bebb548ea31e1574cb146a6e6898030d05144/operations{?cursor,limit,order}",
            instance.Links.Operations.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/liquidity_pools/93f526d8bc7b38ac6d746c2d0a4bebb548ea31e1574cb146a6e6898030d05144",
            instance.Links.Self.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/liquidity_pools/93f526d8bc7b38ac6d746c2d0a4bebb548ea31e1574cb146a6e6898030d05144/transactions{?cursor,limit,order}",
            instance.Links.Transactions.Href);
    }
}