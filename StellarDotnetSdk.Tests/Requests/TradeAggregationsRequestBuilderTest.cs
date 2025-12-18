using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
/// Unit tests for <see cref="TradeAggregationsRequestBuilder"/> class.
/// </summary>
[TestClass]
public class TradeAggregationsRequestBuilderTest
{
    /// <summary>
    /// Verifies that TradeAggregationsRequestBuilder.BuildUri correctly constructs URI with all aggregation parameters.
    /// </summary>
    [TestMethod]
    public void BuildUri_WithAllAggregationParameters_BuildsCorrectUri()
    {
        // Arrange
        var server = new Server("https://horizon-testnet.stellar.org");

        // Act
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

        // Assert
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

    /// <summary>
    /// Verifies that TradeAggregationsRequestBuilder.Execute correctly retrieves and deserializes trade aggregations page data.
    /// </summary>
    [TestMethod]
    public async Task Execute_WithBaseAssetCounterAssetAndTimeRange_ReturnsDeserializedTradeAggregationsPage()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/tradeAggregationPage.json");

        // Act
        var account = await server.TradeAggregations
            .BaseAsset(new AssetTypeNative())
            .CounterAsset(new AssetTypeCreditAlphaNum4("BTC",
                "GATEMHCCKCY67ZUCKTROYN24ZYT5GK4EQZ65JJLDHKHRUZI3EUEKMTCH"))
            .StartTime(1512689100000L)
            .EndTime(1512775500000L)
            .Resolution(300000L)
            .Execute();

        // Assert
        TradeAggregationsPageDeserializerTest.AssertTestData(account);
    }
}