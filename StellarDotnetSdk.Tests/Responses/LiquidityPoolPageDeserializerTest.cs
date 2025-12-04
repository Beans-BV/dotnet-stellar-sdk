using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class LiquidityPoolPageDeserializerTest
{
    [TestMethod]
    public void TestDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("liquidityPoolPage.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<Page<LiquidityPoolResponse>>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertTestData(instance);
    }


    [TestMethod]
    public void TestSerializeDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("liquidityPoolPage.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<Page<LiquidityPoolResponse>>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var parsed = JsonSerializer.Deserialize<Page<LiquidityPoolResponse>>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(parsed);
        AssertTestData(parsed);
    }

    internal static void AssertTestData(Page<LiquidityPoolResponse> poolsPage)
    {
        Assert.IsNotNull(poolsPage.Links);
        Assert.AreEqual("https://horizon-testnet.stellar.org/liquidity_pools?cursor=&limit=10&order=asc",
            poolsPage.Links.Self.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/liquidity_pools?cursor=02d93939463553487c1a427157ef413c27967168f9fff85380ac99b47e82e6da&limit=10&order=asc",
            poolsPage.Links.Next.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/liquidity_pools?cursor=00378e16445a29800e0f9cb4de2c95cf12b2e02740234c527a589a3adc303bde&limit=10&order=desc",
            poolsPage.Links.Prev.Href);

        Assert.AreEqual(30, poolsPage.Records[1].FeeBp);
        Assert.AreEqual("b26c0d6545349ad7f44ba758b7c705459537201583f2e524635be04aff84bc69",
            poolsPage.Records[1].Id.ToString());
        Assert.AreEqual("b26c0d6545349ad7f44ba758b7c705459537201583f2e524635be04aff84bc69",
            poolsPage.Records[1].PagingToken);

        Assert.AreEqual("native", poolsPage.Records[1].Reserves[0].Asset.CanonicalName());
        Assert.AreEqual("0.0000000", poolsPage.Records[1].Reserves[0].Amount);

        Assert.AreEqual("USDC:GAKMOAANQHJKF5735OYVSQZL6KC3VMFL4LP4ZYY2LWK256TSUG45IEFB",
            poolsPage.Records[1].Reserves[1].Asset.CanonicalName());
        Assert.AreEqual("0.0000000", poolsPage.Records[1].Reserves[1].Amount);

        Assert.AreEqual("0.0000000", poolsPage.Records[1].TotalShares);
        Assert.AreEqual("2", poolsPage.Records[1].TotalTrustlines);
        Assert.AreEqual(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
            poolsPage.Records[1].Type);

        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/liquidity_pools/b26c0d6545349ad7f44ba758b7c705459537201583f2e524635be04aff84bc69/operations{?cursor,limit,order}",
            poolsPage.Records[1].Links.Operations.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/liquidity_pools/b26c0d6545349ad7f44ba758b7c705459537201583f2e524635be04aff84bc69",
            poolsPage.Records[1].Links.Self.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/liquidity_pools/b26c0d6545349ad7f44ba758b7c705459537201583f2e524635be04aff84bc69/transactions{?cursor,limit,order}",
            poolsPage.Records[1].Links.Transactions.Href);
    }
}