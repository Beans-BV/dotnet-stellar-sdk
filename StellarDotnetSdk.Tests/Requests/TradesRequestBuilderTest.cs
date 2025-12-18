using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
///     Unit tests for <see cref="TradesRequestBuilder" /> class.
/// </summary>
[TestClass]
public class TradesRequestBuilderTest
{
    /// <summary>
    ///     Verifies that TradesRequestBuilder.BuildUri correctly constructs URI with base asset, counter asset, cursor, limit,
    ///     and order parameters.
    /// </summary>
    [TestMethod]
    public void BuildUri_WithBaseAssetCounterAssetCursorLimitAndOrder_BuildsCorrectUri()
    {
        // Arrange
        var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Trades
            .BaseAsset(Asset.CreateNonNativeAsset("EUR", "GAUPA4HERNBDPVO4IUA3MJXBCRRK5W54EVXTDK6IIUTGDQRB6D5W242W"))
            .CounterAsset(Asset.CreateNonNativeAsset("USD", "GDRRHSJMHXDTQBT4JTCILNGF5AS54FEMTXL7KOLMF6TFTHRK6SSUSUZZ"))
            .Cursor("13537736921089")
            .Limit(200)
            .Order(OrderDirection.ASC)
            .BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/trades?" +
                        "base_asset_type=credit_alphanum4&" +
                        "base_asset_code=EUR&" +
                        "base_asset_issuer=GAUPA4HERNBDPVO4IUA3MJXBCRRK5W54EVXTDK6IIUTGDQRB6D5W242W&" +
                        "counter_asset_type=credit_alphanum4&" +
                        "counter_asset_code=USD&" +
                        "counter_asset_issuer=GDRRHSJMHXDTQBT4JTCILNGF5AS54FEMTXL7KOLMF6TFTHRK6SSUSUZZ&" +
                        "cursor=13537736921089&" +
                        "limit=200&" +
                        "order=asc", uri.ToString());
    }

    /// <summary>
    ///     Verifies that TradesRequestBuilder.Execute correctly retrieves and deserializes trades page data.
    /// </summary>
    [TestMethod]
    public async Task Execute_WithBaseAndCounterAssets_ReturnsDeserializedTradesPage()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/tradePageOrderBook.json");

        // Act
        var trades = await server.Trades
            .BaseAsset(new AssetTypeCreditAlphaNum4("EUR",
                "GAUPA4HERNBDPVO4IUA3MJXBCRRK5W54EVXTDK6IIUTGDQRB6D5W242W"))
            .CounterAsset(new AssetTypeCreditAlphaNum4("USD",
                "GDRRHSJMHXDTQBT4JTCILNGF5AS54FEMTXL7KOLMF6TFTHRK6SSUSUZZ"))
            .Execute();

        // Assert
        TradesPageDeserializerTest.AssertOrderBookTrade(trades);
    }

    /// <summary>
    ///     Verifies that TradesRequestBuilder.ForAccount correctly constructs URI for account trades.
    /// </summary>
    [TestMethod]
    public void ForAccount_WithValidAccountId_BuildsCorrectUri()
    {
        // Arrange
        var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Trades
            .ForAccount("GDRRHSJMHXDTQBT4JTCILNGF5AS54FEMTXL7KOLMF6TFTHRK6SSUSUZZ")
            .Cursor("13537736921089")
            .Limit(200)
            .Order(OrderDirection.ASC)
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GDRRHSJMHXDTQBT4JTCILNGF5AS54FEMTXL7KOLMF6TFTHRK6SSUSUZZ/trades?" +
            "cursor=13537736921089&" +
            "limit=200&" +
            "order=asc", uri.ToString());
    }
}