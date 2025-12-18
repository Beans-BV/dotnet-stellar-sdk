using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
/// Unit tests for <see cref="OffersRequestBuilder"/> class.
/// </summary>
[TestClass]
public class OffersRequestBuilderTest
{
    /// <summary>
    /// Verifies that OffersRequestBuilder.ForAccount correctly constructs URI for account offers.
    /// </summary>
    [TestMethod]
    public void ForAccount_WithValidAccountId_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Offers
            .ForAccount("GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H")
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H/offers?limit=200&order=desc",
            uri.ToString());
    }

    /// <summary>
    /// Verifies that OffersRequestBuilder.Execute correctly retrieves and deserializes offer page data.
    /// </summary>
    [TestMethod]
    public async Task Execute_ForAccount_ReturnsDeserializedOfferPage()
    {
        // Arrange
        var server = await CreateServer();

        // Act
        var offerResponsePage = await server.Offers
            .ForAccount("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7")
            .Execute();

        // Assert
        OfferPageDeserializerTest.AssertTestData(offerResponsePage);
    }

    /// <summary>
    /// Verifies that OffersRequestBuilder.WithSeller correctly filters offers by seller and returns matching offers.
    /// </summary>
    [TestMethod]
    public async Task WithSeller_WithValidSellerId_ReturnsMatchingOffers()
    {
        // Arrange
        var server = await CreateServer();

        // Act
        var req = server.Offers.WithSeller("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7");
        var offerResponsePage = await req.Execute();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/offers?seller=GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7",
            req.BuildUri().ToString());
        OfferPageDeserializerTest.AssertTestData(offerResponsePage);
    }

    /// <summary>
    /// Verifies that OffersRequestBuilder.WithSellingAsset correctly filters offers by selling native asset.
    /// </summary>
    [TestMethod]
    public async Task WithSellingAsset_WithNativeAsset_ReturnsMatchingOffers()
    {
        // Arrange
        var server = await CreateServer();

        // Act
        var req = server.Offers.WithSellingAsset(new AssetTypeNative());
        var offerResponsePage = await req.Execute();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/offers?selling_asset_type=native",
            req.BuildUri().ToString());
        OfferPageDeserializerTest.AssertTestData(offerResponsePage);
    }

    /// <summary>
    /// Verifies that OffersRequestBuilder.WithSellingAsset correctly filters offers by selling credit asset.
    /// </summary>
    [TestMethod]
    public async Task WithSellingAsset_WithCreditAsset_ReturnsMatchingOffers()
    {
        // Arrange
        var nonNativeAsset =
            Asset.CreateNonNativeAsset("FOO", "GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7");
        var server = await CreateServer();

        // Act
        var req = server.Offers.WithSellingAsset(nonNativeAsset);
        var offerResponsePage = await req.Execute();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/offers?selling_asset_type=credit_alphanum4&selling_asset_code=FOO&selling_asset_issuer=GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7",
            req.BuildUri().ToString());
        OfferPageDeserializerTest.AssertTestData(offerResponsePage);
    }

    /// <summary>
    /// Verifies that OffersRequestBuilder.WithBuyingAsset correctly filters offers by buying native asset.
    /// </summary>
    [TestMethod]
    public async Task WithBuyingAsset_WithNativeAsset_ReturnsMatchingOffers()
    {
        // Arrange
        var server = await CreateServer();

        // Act
        var req = server.Offers.WithBuyingAsset(new AssetTypeNative());
        var offerResponsePage = await req.Execute();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/offers?buying_asset_type=native",
            req.BuildUri().ToString());
        OfferPageDeserializerTest.AssertTestData(offerResponsePage);
    }

    /// <summary>
    /// Verifies that OffersRequestBuilder.WithBuyingAsset correctly filters offers by buying credit asset.
    /// </summary>
    [TestMethod]
    public async Task WithBuyingAsset_WithCreditAsset_ReturnsMatchingOffers()
    {
        // Arrange
        var notNativeAsset = Asset.CreateNonNativeAsset("FOOBARBAZ",
            "GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7");
        var server = await CreateServer();

        // Act
        var req = server.Offers.WithBuyingAsset(notNativeAsset);
        var offerResponsePage = await req.Execute();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/offers?buying_asset_type=credit_alphanum12&buying_asset_code=FOOBARBAZ&buying_asset_issuer=GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7",
            req.BuildUri().ToString());
        OfferPageDeserializerTest.AssertTestData(offerResponsePage);
    }

    /// <summary>
    /// Verifies that OffersRequestBuilder.Offer correctly retrieves and deserializes single offer data.
    /// </summary>
    [TestMethod]
    public async Task Offer_WithValidOfferId_ReturnsDeserializedOffer()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/offer.json");

        // Act
        var offer = await server.Offers.Offer("GAHEAZQD6K7QBBPGLQZQOHCA5L3Z7ZNFBDU5XCCEI7QYVEFIRUF3IW27");

        // Assert
        Assert.AreEqual("100.0000000", offer.Amount);
        Assert.AreEqual("1", offer.PagingToken);
        Assert.AreEqual(0, offer.Buying.CompareTo(new AssetTypeNative()));
        Assert.AreEqual("GAHEAZQD6K7QBBPGLQZQOHCA5L3Z7ZNFBDU5XCCEI7QYVEFIRUF3IW27", offer.Seller);
        Assert.AreEqual(0,
            offer.Selling.CompareTo(Asset.CreateNonNativeAsset("IOM",
                "GBL37ZFYU3FRBKTFO4YPIAELTO2PN6YEIF3ZS6FKOEAM5SOWALX5IP44")));
        Assert.AreEqual("1", offer.Id);
        Assert.AreEqual("https://horizon-testnet.stellar.org/offers/1", offer.Links.Self.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GAHEAZQD6K7QBBPGLQZQOHCA5L3Z7ZNFBDU5XCCEI7QYVEFIRUF3IW27",
            offer.Links.OfferMaker.Href);
        Assert.AreEqual(1L, offer.PriceRatio.Numerator);
        Assert.AreEqual(2L, offer.PriceRatio.Denominator);
        Assert.AreEqual("0.5000000", offer.Price);
        Assert.AreEqual(380, offer.LastModifiedLedger);
        Assert.AreEqual(new DateTimeOffset(2024, 12, 10, 17, 50, 8, TimeSpan.Zero), offer.LastModifiedTime);
    }

    private async Task<Server> CreateServer()
    {
        return await Utils.CreateTestServerWithJson("Responses/offerPage.json");
    }
}