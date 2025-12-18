using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

/// <summary>
/// Unit tests for deserializing order book responses from JSON.
/// </summary>
[TestClass]
public class OrderBookDeserializerTest
{
    /// <summary>
    /// Verifies that OrderBookResponse can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithOrderBookJson_ReturnsDeserializedOrderBook()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("orderBook.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var orderBook = JsonSerializer.Deserialize<OrderBookResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(orderBook);
        AssertTestData(orderBook);
    }

    /// <summary>
    /// Verifies that OrderBookResponse can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithOrderBook_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("orderBook.json");
        var json = File.ReadAllText(jsonPath);
        var orderBook = JsonSerializer.Deserialize<OrderBookResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(orderBook);
        var back = JsonSerializer.Deserialize<OrderBookResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertTestData(back);
    }

    public static void AssertTestData(OrderBookResponse orderBook)
    {
        Assert.AreEqual(orderBook.OrderBookBase, new AssetTypeNative());
        Assert.AreEqual(orderBook.Counter,
            Asset.CreateNonNativeAsset("DEMO", "GBAMBOOZDWZPVV52RCLJQYMQNXOBLOXWNQAY2IF2FREV2WL46DBCH3BE"));

        Assert.AreEqual(orderBook.Bids[0].Amount, "31.4007644");
        Assert.AreEqual(orderBook.Bids[0].Price, "0.0024224");
        Assert.AreEqual(orderBook.Bids[0].PriceRatio.Numerator, 4638606);
        Assert.AreEqual(orderBook.Bids[0].PriceRatio.Denominator, 1914900241);

        Assert.AreEqual(orderBook.Bids[1].Amount, "5.9303650");
        Assert.AreEqual(orderBook.Bids[1].Price, "0.0024221");

        Assert.AreEqual(orderBook.Asks[0].Amount, "541.4550766");
        Assert.AreEqual(orderBook.Asks[0].Price, "0.0025093");

        Assert.AreEqual(orderBook.Asks[1].Amount, "121.9999600");
        Assert.AreEqual(orderBook.Asks[1].Price, "0.0025093");
    }
}