using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
/// Unit tests for <see cref="OrderBookRequestBuilder"/> class.
/// </summary>
[TestClass]
public class OrderBookRequestBuilderTest
{
    /// <summary>
    /// Verifies that OrderBookRequestBuilder.BuildUri correctly constructs URI with buying and selling credit assets.
    /// </summary>
    [TestMethod]
    public void BuildUri_WithBuyingAndSellingCreditAssets_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.OrderBook
            .BuyingAsset(Asset.CreateNonNativeAsset("EUR",
                "GAUPA4HERNBDPVO4IUA3MJXBCRRK5W54EVXTDK6IIUTGDQRB6D5W242W"))
            .SellingAsset(Asset.CreateNonNativeAsset("USD",
                "GDRRHSJMHXDTQBT4JTCILNGF5AS54FEMTXL7KOLMF6TFTHRK6SSUSUZZ"))
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/order_book?" +
            "buying_asset_type=credit_alphanum4&" +
            "buying_asset_code=EUR&" +
            "buying_asset_issuer=GAUPA4HERNBDPVO4IUA3MJXBCRRK5W54EVXTDK6IIUTGDQRB6D5W242W&" +
            "selling_asset_type=credit_alphanum4&" +
            "selling_asset_code=USD&" +
            "selling_asset_issuer=GDRRHSJMHXDTQBT4JTCILNGF5AS54FEMTXL7KOLMF6TFTHRK6SSUSUZZ",
            uri.ToString());
    }

    /// <summary>
    /// Verifies that OrderBookRequestBuilder.Execute correctly retrieves and deserializes order book data.
    /// </summary>
    [TestMethod]
    public async Task Execute_WithBuyingAndSellingAssets_ReturnsDeserializedOrderBook()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/orderBook.json");

        // Act
        var orderBookPage = await server.OrderBook
            .BuyingAsset(new AssetTypeNative())
            .SellingAsset(new AssetTypeCreditAlphaNum4("DEMO",
                "GC3BVJOU7SHHFLZ2LDYW6JU4YW36R2MRF6C37QJWQXZWG3JBYNODGHOB"))
            .Execute();

        // Assert
        OrderBookDeserializerTest.AssertTestData(orderBookPage);
    }
}