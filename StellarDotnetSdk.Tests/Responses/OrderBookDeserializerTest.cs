using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class OrderBookDeserializerTest
{
    [TestMethod]
    public void TestDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("orderBook.json");
        var json = File.ReadAllText(jsonPath);
        var orderBook = JsonSerializer.Deserialize<OrderBookResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(orderBook);
        AssertTestData(orderBook);
    }

    [TestMethod]
    public void TestSerializeDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("orderBook.json");
        var json = File.ReadAllText(jsonPath);
        var orderBook = JsonSerializer.Deserialize<OrderBookResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(orderBook);
        var back = JsonSerializer.Deserialize<OrderBookResponse>(serialized);
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
        Assert.AreEqual(orderBook.Bids[0].PriceR.Numerator, 4638606);
        Assert.AreEqual(orderBook.Bids[0].PriceR.Denominator, 1914900241);

        Assert.AreEqual(orderBook.Bids[1].Amount, "5.9303650");
        Assert.AreEqual(orderBook.Bids[1].Price, "0.0024221");

        Assert.AreEqual(orderBook.Asks[0].Amount, "541.4550766");
        Assert.AreEqual(orderBook.Asks[0].Price, "0.0025093");

        Assert.AreEqual(orderBook.Asks[1].Amount, "121.9999600");
        Assert.AreEqual(orderBook.Asks[1].Price, "0.0025093");
    }
}