using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

/// <summary>
///     Unit tests for deserializing trade aggregations page responses from JSON.
/// </summary>
[TestClass]
public class TradeAggregationsPageDeserializerTest
{
    /// <summary>
    ///     Verifies that Page&lt;TradeAggregationResponse&gt; can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithTradeAggregationsPageJson_ReturnsDeserializedTradeAggregationsPage()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("tradeAggregationPage.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var tradeAggregationsPage =
            JsonSerializer.Deserialize<Page<TradeAggregationResponse>>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(tradeAggregationsPage);
        AssertTestData(tradeAggregationsPage);
    }

    /// <summary>
    ///     Verifies that Page&lt;TradeAggregationResponse&gt; can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithTradeAggregationsPage_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("tradeAggregationPage.json");
        var json = File.ReadAllText(jsonPath);
        var tradeAggregationsPage =
            JsonSerializer.Deserialize<Page<TradeAggregationResponse>>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(tradeAggregationsPage);
        var back = JsonSerializer.Deserialize<Page<TradeAggregationResponse>>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertTestData(back);
    }

    public static void AssertTestData(Page<TradeAggregationResponse> tradeAggregationsPage)
    {
        Assert.AreEqual(tradeAggregationsPage.Links.Next.Href,
            "https://horizon.stellar.org/trade_aggregations?base_asset_type=native&counter_asset_code=SLT&counter_asset_issuer=GCKA6K5PCQ6PNF5RQBF7PQDJWRHO6UOGFMRLK3DYHDOI244V47XKQ4GP&counter_asset_type=credit_alphanum4&end_time=1517532526000&limit=200&order=asc&resolution=3600000&start_time=1517529600000");
        Assert.AreEqual(tradeAggregationsPage.Links.Self.Href,
            "https://horizon.stellar.org/trade_aggregations?base_asset_type=native&counter_asset_code=SLT&counter_asset_issuer=GCKA6K5PCQ6PNF5RQBF7PQDJWRHO6UOGFMRLK3DYHDOI244V47XKQ4GP&counter_asset_type=credit_alphanum4&limit=200&order=asc&resolution=3600000&start_time=1517521726000&end_time=1517532526000");

        var record = tradeAggregationsPage.Records[0];
        Assert.AreEqual("1517522400000", record.Timestamp);
        Assert.AreEqual("26", record.TradeCount);
        Assert.AreEqual("27575.0201596", record.BaseVolume);
        Assert.AreEqual("5085.6410385", record.CounterVolume);
        Assert.AreEqual("0.1844293", record.Avg);
        Assert.AreEqual("0.1915709", record.High);
        Assert.AreEqual(50L, record.HighR.Numerator);
        Assert.AreEqual(261L, record.HighR.Denominator);
        Assert.AreEqual("0.1506024", record.Low);
        Assert.AreEqual(25L, record.LowR.Numerator);
        Assert.AreEqual(166L, record.LowR.Denominator);
        Assert.AreEqual("0.1724138", record.Open);
        Assert.AreEqual(5L, record.OpenR.Numerator);
        Assert.AreEqual(29L, record.OpenR.Denominator);
        Assert.AreEqual("0.1506024", record.Close);
        Assert.AreEqual(25L, record.CloseR.Numerator);
        Assert.AreEqual(166L, record.CloseR.Denominator);
    }
}