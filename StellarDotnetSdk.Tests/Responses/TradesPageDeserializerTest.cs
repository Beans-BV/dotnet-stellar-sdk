using System;
using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class TradesPageDeserializerTest
{
    [TestMethod]
    public void TestDeserializePageOfOrderBookTrade()
    {
        var jsonPath = Utils.GetTestDataPath("tradePageOrderBook.json");
        var json = File.ReadAllText(jsonPath);
        var tradesPage = JsonSerializer.Deserialize<Page<TradeResponse>>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(tradesPage);
        AssertOrderBookTrade(tradesPage);
    }

    [TestMethod]
    public void TestSerializeDeserializePageOfOrderBookTrade()
    {
        var jsonPath = Utils.GetTestDataPath("tradePageOrderBook.json");
        var json = File.ReadAllText(jsonPath);
        var tradesPage = JsonSerializer.Deserialize<Page<TradeResponse>>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(tradesPage);
        var back = JsonSerializer.Deserialize<Page<TradeResponse>>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertOrderBookTrade(back);
    }

    [TestMethod]
    public void TestDeserializePageOfLiquidityPoolTrades()
    {
        var jsonPath = Utils.GetTestDataPath("tradePageLiquidityPool.json");
        var json = File.ReadAllText(jsonPath);
        var tradesPage = JsonSerializer.Deserialize<Page<TradeResponse>>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(tradesPage);
        AssertLiquidityPoolTrades(tradesPage);
    }

    [TestMethod]
    public void TestSerializeDeserializePageOfLiquidityPoolTrades()
    {
        var jsonPath = Utils.GetTestDataPath("tradePageLiquidityPool.json");
        var json = File.ReadAllText(jsonPath);
        var tradesPage = JsonSerializer.Deserialize<Page<TradeResponse>>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(tradesPage);
        var back = JsonSerializer.Deserialize<Page<TradeResponse>>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertLiquidityPoolTrades(back);
    }

    public static void AssertOrderBookTrade(Page<TradeResponse> tradesPage)
    {
        Assert.AreEqual(tradesPage.Links.Self.Href,
            "https://horizon-testnet.stellar.org/trades?cursor=&limit=10&offer_id=24&order=asc");
        Assert.AreEqual(tradesPage.Links.Next.Href,
            "https://horizon-testnet.stellar.org/trades?cursor=6816113102849-0&limit=10&offer_id=24&order=asc");
        Assert.AreEqual(tradesPage.Links.Prev.Href,
            "https://horizon-testnet.stellar.org/trades?cursor=6816113102849-0&limit=10&offer_id=24&order=desc");

        Assert.IsNotNull(tradesPage.Records);
        Assert.AreEqual(1, tradesPage.Records.Count);

        var record = tradesPage.Records[0];
        Assert.IsNotNull(record.Links);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GBLD7NSGWYT2KRFTYMRJGQ42SNTGDXMZYNQUE3H7N2VEMARIQYKBB4JJ",
            record.Links.Base.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GAG3ZVX4564SWFE5YIEHVHNAOBH7XTPTYC6R7U6GCJ5RYATQIOPGTEST",
            record.Links.Counter.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/operations/6816113102849", record.Links.Operation.Href);

        Assert.AreEqual("6816113102849-0", record.Id);
        Assert.AreEqual("6816113102849-0", record.PagingToken);
        Assert.AreEqual("2025-08-14T19:35:23Z", record.LedgerCloseTime);
        Assert.AreEqual("orderbook", record.TradeType);
        Assert.AreEqual("24", record.BaseOfferId);
        Assert.AreEqual("GBLD7NSGWYT2KRFTYMRJGQ42SNTGDXMZYNQUE3H7N2VEMARIQYKBB4JJ", record.BaseAccount);
        Assert.AreEqual("2222.2222211", record.BaseAmount);
        Assert.AreEqual("credit_alphanum4", record.BaseAssetType);
        Assert.AreEqual("TEST", record.BaseAssetCode);
        Assert.AreEqual("GB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST", record.BaseAssetIssuer);
        Assert.AreEqual(Asset.Create("TEST:GB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST"),
            record.BaseAsset);
        Assert.AreEqual("4611692834540490753", record.CounterOfferId);
        Assert.AreEqual("GAG3ZVX4564SWFE5YIEHVHNAOBH7XTPTYC6R7U6GCJ5RYATQIOPGTEST", record.CounterAccount);
        Assert.AreEqual("199.9999999", record.CounterAmount);
        Assert.AreEqual("native", record.CounterAssetType);
        Assert.IsNull(record.CounterAssetCode);
        Assert.IsNull(record.CounterAssetIssuer);
        Assert.AreEqual(Asset.Create("native"), record.CounterAsset);
        Assert.AreEqual(true, record.BaseIsSeller);
        Assert.IsNull(record.LiquidityPoolFeeBp);
        Assert.IsNull(record.BaseLiquidityPoolId);
        Assert.IsNull(record.CounterLiquidityPoolId);

        // Price
        Assert.IsNotNull(record.Price);

        Assert.AreEqual("9", record.Price.Numerator);
        Assert.AreEqual("100", record.Price.Denominator);
    }

    private static void AssertLiquidityPoolTrades(Page<TradeResponse> tradesPage)
    {
        Assert.AreEqual(tradesPage.Links.Self.Href,
            "https://horizon-testnet.stellar.org/trades?cursor=&limit=10&order=asc");
        Assert.AreEqual(tradesPage.Links.Next.Href,
            "https://horizon-testnet.stellar.org/trades?cursor=10033043628036-0&limit=10&order=asc");
        Assert.AreEqual(tradesPage.Links.Prev.Href,
            "https://horizon-testnet.stellar.org/trades?cursor=6816113102849-0&limit=10&order=desc");

        Assert.IsNotNull(tradesPage.Records);
        Assert.AreEqual(2, tradesPage.Records.Count);

        // Record1: BaseOfferId <->  CounterLiquidityPoolId
        var record1 = tradesPage.Records[0];
        Assert.IsNotNull(record1.Links);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GALMNOMXPF2FW6XOQ5SKX3T43WMYGRZMLCMZ6H4WGBE47JSNMZRWOEPJ",
            record1.Links.Base.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/liquidity_pools/93f526d8bc7b38ac6d746c2d0a4bebb548ea31e1574cb146a6e6898030d05144",
            record1.Links.Counter.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/operations/7202660159489", record1.Links.Operation.Href);

        Assert.AreEqual("7202660159489-0", record1.Id);
        Assert.AreEqual("7202660159489-0", record1.PagingToken);
        Assert.AreEqual("2025-08-14T19:42:53Z", record1.LedgerCloseTime);
        Assert.AreEqual("liquidity_pool", record1.TradeType);
        Assert.AreEqual("4611693221087547393", record1.BaseOfferId);
        Assert.AreEqual(30U, record1.LiquidityPoolFeeBp);
        Assert.AreEqual("GALMNOMXPF2FW6XOQ5SKX3T43WMYGRZMLCMZ6H4WGBE47JSNMZRWOEPJ", record1.BaseAccount);
        Assert.AreEqual("10.0000000", record1.BaseAmount);
        Assert.AreEqual("native", record1.BaseAssetType);
        Assert.IsNull(record1.BaseAssetIssuer);
        Assert.IsNull(record1.BaseAssetCode);
        Assert.AreEqual(Asset.Create("native"), record1.BaseAsset);

        Assert.IsNotNull(record1.CounterLiquidityPoolId);
        Assert.AreEqual("93f526d8bc7b38ac6d746c2d0a4bebb548ea31e1574cb146a6e6898030d05144",
            record1.CounterLiquidityPoolId.ToString());
        Assert.IsNull(record1.CounterOfferId);
        Assert.IsNull(record1.CounterAccount);
        Assert.AreEqual("105.5177194", record1.CounterAmount);
        Assert.AreEqual("credit_alphanum4", record1.CounterAssetType);
        Assert.AreEqual("TEST", record1.CounterAssetCode);
        Assert.AreEqual("GC6ZBHGJGGTPVLYALOKQNQSQUXHJUYDZ7VLMAPU2MERVTYMKVL2GTEST", record1.CounterAssetIssuer);
        Assert.AreEqual(Asset.Create("TEST:GC6ZBHGJGGTPVLYALOKQNQSQUXHJUYDZ7VLMAPU2MERVTYMKVL2GTEST"),
            record1.CounterAsset);
        Assert.AreEqual(false, record1.BaseIsSeller);

        Assert.AreEqual(tradesPage.Records[0].LedgerCloseTime,
            new DateTimeOffset(2018, 2, 2, 0, 20, 10, TimeSpan.Zero));
        Assert.AreEqual(tradesPage.Records[0].OfferId, "695254");
        Assert.AreEqual(tradesPage.Records[0].BaseOfferId, "10");
        Assert.AreEqual(tradesPage.Records[0].CounterOfferId, "11");
        Assert.AreEqual(tradesPage.Records[0].BaseAccount, "GBZXCJIUEPDXGHMS64UBJHUVKV6ETWYOVHADLTBXJNJFUC7A7RU5B3GN");
        Assert.AreEqual(tradesPage.Records[0].BaseAmount, "0.1217566");
        Assert.AreEqual(tradesPage.Records[0].BaseAssetType, "native");
        Assert.AreEqual(tradesPage.Records[0].CounterAccount,
            "GBHKUQDYXGK5IEYORI7DZMMXANOIEHHOF364LNT4Q7EWPUL7FOO2SP6D");
        Assert.AreEqual(tradesPage.Records[0].CounterAmount, "0.0199601");
        Assert.AreEqual(tradesPage.Records[0].CounterAssetType, "credit_alphanum4");
        Assert.AreEqual(tradesPage.Records[0].CounterAssetCode, "SLT");
        Assert.AreEqual(tradesPage.Records[0].CounterAssetIssuer,
            "GCKA6K5PCQ6PNF5RQBF7PQDJWRHO6UOGFMRLK3DYHDOI244V47XKQ4GP");
        Assert.AreEqual(tradesPage.Records[0].BaseIsSeller, true);

        Assert.AreEqual("1055177194", record1.Price.Numerator);
        Assert.AreEqual("100000000", record1.Price.Denominator);

        // Record2: CounterLiquidityPoolId <->  BaseOfferId
        var record2 = tradesPage.Records[1];
        Assert.IsNotNull(record2.Links);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/liquidity_pools/b6564a56fbec0bf6d7c3894f34287339ab5d3d3012aa3180dfade15f9104bffd",
            record2.Links.Base.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GDOVNWOX5DKFMACQT2ZGHECTTIPSBZ4AT3SCP2RF5BKHNDBUDGNIAJXS",
            record2.Links.Counter.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/operations/54593329328129", record2.Links.Operation.Href);

        Assert.AreEqual("54593329328129-0", record2.Id);
        Assert.AreEqual("54593329328129-0", record2.PagingToken);
        Assert.AreEqual("2025-08-15T11:03:36Z", record2.LedgerCloseTime);
        Assert.IsNotNull(record2.BaseLiquidityPoolId);
        Assert.AreEqual("b6564a56fbec0bf6d7c3894f34287339ab5d3d3012aa3180dfade15f9104bffd",
            record2.BaseLiquidityPoolId.ToString());
        Assert.AreEqual(30U, record2.LiquidityPoolFeeBp);
        Assert.AreEqual("liquidity_pool", record2.TradeType);
        Assert.IsNull(record2.BaseOfferId);
        Assert.IsNull(record2.BaseAccount);
        Assert.AreEqual("9.3486278", record2.BaseAmount);
        Assert.AreEqual("credit_alphanum12", record2.BaseAssetType);
        Assert.AreEqual("PHPSTAR", record2.BaseAssetCode);
        Assert.AreEqual("GBR5QBVY7IPWICDRNNKGYNCOTBSFQMXBMS5VHC26C6REEVTXHRQ5T4LM", record2.BaseAssetIssuer);
        Assert.AreEqual(Asset.Create("PHPSTAR:GBR5QBVY7IPWICDRNNKGYNCOTBSFQMXBMS5VHC26C6REEVTXHRQ5T4LM"),
            record2.BaseAsset);

        Assert.IsNull(record2.CounterLiquidityPoolId);
        Assert.AreEqual("4611740611756716033", record2.CounterOfferId);
        Assert.AreEqual("GDOVNWOX5DKFMACQT2ZGHECTTIPSBZ4AT3SCP2RF5BKHNDBUDGNIAJXS", record2.CounterAccount);
        Assert.AreEqual("10.0000000", record2.CounterAmount);
        Assert.AreEqual("credit_alphanum4", record2.CounterAssetType);
        Assert.AreEqual("SDK", record2.CounterAssetCode);
        Assert.AreEqual("GDXKHDGM4X3YB6YH4UXG3NYDLFQSP5HUHYFX4AH4HYGIE5PEBGINWWEI", record2.CounterAssetIssuer);
        Assert.AreEqual(Asset.Create("SDK:GDXKHDGM4X3YB6YH4UXG3NYDLFQSP5HUHYFX4AH4HYGIE5PEBGINWWEI"),
            record2.CounterAsset);
        Assert.AreEqual(true, record2.BaseIsSeller);

        // Price
        Assert.IsNotNull(record2.Price);

        Assert.AreEqual("100000000", record2.Price.Numerator);
        Assert.AreEqual("93486278", record2.Price.Denominator);
    }
}