using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Assets;
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
        var instance = JsonSingleton.GetInstance<LiquidityPoolResponse>(json);
        Assert.IsNotNull(instance);
        Assert.AreEqual(new LiquidityPoolID("67260c4c1807b262ff851b0a3fe141194936bb0215b2f77447f1df11998eabb9"),
            instance.Id);
        Assert.AreEqual("113725249324879873", instance.PagingToken);
        Assert.AreEqual(30, instance.FeeBp);
        Assert.AreEqual(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, instance.Type);
        Assert.AreEqual("300", instance.TotalTrustlines);
        Assert.AreEqual("5000.0000000", instance.TotalShares);

        Assert.AreEqual("1000.0000005", instance.Reserves[0].Amount);
        Assert.AreEqual("EURT:GAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S",
            instance.Reserves[0].Asset.CanonicalName());

        Assert.AreEqual("2000.0000000", instance.Reserves[1].Amount);
        Assert.AreEqual("PHP:GAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S",
            instance.Reserves[1].Asset.CanonicalName());

        Assert.AreEqual(
            "/liquidity_pools/67260c4c1807b262ff851b0a3fe141194936bb0215b2f77447f1df11998eabb9/effects{?cursor,limit,order}",
            instance.Links.Effects.Href);
        Assert.AreEqual(
            "/liquidity_pools/67260c4c1807b262ff851b0a3fe141194936bb0215b2f77447f1df11998eabb9/operations{?cursor,limit,order}",
            instance.Links.Operations.Href);
        Assert.AreEqual("/liquidity_pools/67260c4c1807b262ff851b0a3fe141194936bb0215b2f77447f1df11998eabb9",
            instance.Links.Self.Href);
        Assert.AreEqual(
            "/liquidity_pools/67260c4c1807b262ff851b0a3fe141194936bb0215b2f77447f1df11998eabb9/transactions{?cursor,limit,order}",
            instance.Links.Transactions.Href);
    }

    [TestMethod]
    public void TestSerializeDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("liquidityPool.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<LiquidityPoolResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var parsed = JsonConvert.DeserializeObject<LiquidityPoolResponse>(serialized);
        Assert.IsNotNull(parsed);
        Assert.AreEqual(new LiquidityPoolID("67260c4c1807b262ff851b0a3fe141194936bb0215b2f77447f1df11998eabb9"),
            parsed.Id);
        Assert.AreEqual("113725249324879873", parsed.PagingToken);
        Assert.AreEqual(30, parsed.FeeBp);
        Assert.AreEqual(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, parsed.Type);
        Assert.AreEqual("300", parsed.TotalTrustlines);
        Assert.AreEqual("5000.0000000", parsed.TotalShares);

        Assert.AreEqual("1000.0000005", parsed.Reserves[0].Amount);
        Assert.AreEqual("EURT:GAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S",
            parsed.Reserves[0].Asset.CanonicalName());

        Assert.AreEqual("2000.0000000", parsed.Reserves[1].Amount);
        Assert.AreEqual("PHP:GAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S",
            parsed.Reserves[1].Asset.CanonicalName());

        Assert.AreEqual(
            "/liquidity_pools/67260c4c1807b262ff851b0a3fe141194936bb0215b2f77447f1df11998eabb9/effects{?cursor,limit,order}",
            parsed.Links.Effects.Href);
        Assert.AreEqual(
            "/liquidity_pools/67260c4c1807b262ff851b0a3fe141194936bb0215b2f77447f1df11998eabb9/operations{?cursor,limit,order}",
            parsed.Links.Operations.Href);
        Assert.AreEqual("/liquidity_pools/67260c4c1807b262ff851b0a3fe141194936bb0215b2f77447f1df11998eabb9",
            parsed.Links.Self.Href);
        Assert.AreEqual(
            "/liquidity_pools/67260c4c1807b262ff851b0a3fe141194936bb0215b2f77447f1df11998eabb9/transactions{?cursor,limit,order}",
            parsed.Links.Transactions.Href);
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
}