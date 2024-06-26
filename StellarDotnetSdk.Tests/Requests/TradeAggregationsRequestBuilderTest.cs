﻿using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

[TestClass]
public class TradeAggregationsRequestBuilderTest
{
    [TestMethod]
    public void TestTradeAggregations()
    {
        var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.TradeAggregations
            .BaseAsset(new AssetTypeNative())
            .CounterAsset(Asset.CreateNonNativeAsset("BTC", "GATEMHCCKCY67ZUCKTROYN24ZYT5GK4EQZ65JJLDHKHRUZI3EUEKMTCH"))
            .StartTime(1512689100000L)
            .EndTime(1512775500000L)
            .Resolution(300000L)
            .Offset(3600L)
            .Limit(200)
            .Order(OrderDirection.ASC)
            .BuildUri();

        Assert.AreEqual(uri.ToString(),
            "https://horizon-testnet.stellar.org/trade_aggregations?" +
            "base_asset_type=native&" +
            "counter_asset_type=credit_alphanum4&" +
            "counter_asset_code=BTC&" +
            "counter_asset_issuer=GATEMHCCKCY67ZUCKTROYN24ZYT5GK4EQZ65JJLDHKHRUZI3EUEKMTCH&" +
            "start_time=1512689100000&" +
            "end_time=1512775500000&" +
            "resolution=300000&" +
            "offset=3600&" +
            "limit=200&" +
            "order=asc");
    }

    [TestMethod]
    public async Task TestTradeAggregationsExecute()
    {
        using var server = await Utils.CreateTestServerWithJson("Responses/tradeAggregationPage.json");
        var account = await server.TradeAggregations
            .BaseAsset(new AssetTypeNative())
            .CounterAsset(new AssetTypeCreditAlphaNum4("BTC",
                "GATEMHCCKCY67ZUCKTROYN24ZYT5GK4EQZ65JJLDHKHRUZI3EUEKMTCH"))
            .StartTime(1512689100000L)
            .EndTime(1512775500000L)
            .Resolution(300000L)
            .Execute();

        TradeAggregationsPageDeserializerTest.AssertTestData(account);
    }
}