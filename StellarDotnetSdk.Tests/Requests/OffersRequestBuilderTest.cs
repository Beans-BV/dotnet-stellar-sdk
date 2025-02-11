using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

[TestClass]
public class OffersRequestBuilderTest
{
    [TestMethod]
    public void TestForAccount()
    {
        using var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.Offers
            .ForAccount("GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H")
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H/offers?limit=200&order=desc",
            uri.ToString());
    }

    [TestMethod]
    public async Task TestOffersExecute()
    {
        var server = await CreateServer();
        var offerResponsePage = await server.Offers
            .ForAccount("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7")
            .Execute();

        OfferPageDeserializerTest.AssertTestData(offerResponsePage);
    }

    [TestMethod]
    public async Task TestOffersWithSeller()
    {
        var server = await CreateServer();
        var req = server.Offers.WithSeller("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7");

        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/offers?seller=GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7",
            req.BuildUri().ToString());

        var offerResponsePage = await req.Execute();
        OfferPageDeserializerTest.AssertTestData(offerResponsePage);
    }

    [TestMethod]
    public async Task TestOffersWithSellingNativeAsset()
    {
        var server = await CreateServer();
        var req = server.Offers.WithSellingAsset(new AssetTypeNative());

        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/offers?selling_asset_type=native",
            req.BuildUri().ToString());

        var offerResponsePage = await req.Execute();
        OfferPageDeserializerTest.AssertTestData(offerResponsePage);
    }

    [TestMethod]
    public async Task TestOffersWithSellingCreditAsset()
    {
        var nonNativeAsset =
            Asset.CreateNonNativeAsset("FOO", "GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7");
        var server = await CreateServer();
        var req = server.Offers.WithSellingAsset(nonNativeAsset);

        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/offers?selling_asset_type=credit_alphanum4&selling_asset_code=FOO&selling_asset_issuer=GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7",
            req.BuildUri().ToString());

        var offerResponsePage = await req.Execute();
        OfferPageDeserializerTest.AssertTestData(offerResponsePage);
    }

    [TestMethod]
    public async Task TestOffersWithBuyingNativeAsset()
    {
        var server = await CreateServer();
        var req = server.Offers.WithBuyingAsset(new AssetTypeNative());

        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/offers?buying_asset_type=native",
            req.BuildUri().ToString());

        var offerResponsePage = await req.Execute();
        OfferPageDeserializerTest.AssertTestData(offerResponsePage);
    }

    [TestMethod]
    public async Task TestOffersWithBuyingCreditAsset()
    {
        var notNativeAsset = Asset.CreateNonNativeAsset("FOOBARBAZ",
            "GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7");
        var server = await CreateServer();
        var req = server.Offers.WithBuyingAsset(notNativeAsset);

        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/offers?buying_asset_type=credit_alphanum12&buying_asset_code=FOOBARBAZ&buying_asset_issuer=GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7",
            req.BuildUri().ToString());

        var offerResponsePage = await req.Execute();
        OfferPageDeserializerTest.AssertTestData(offerResponsePage);
    }

    [TestMethod]
    public async Task TestSingleOffer()
    {
        using var server = await Utils.CreateTestServerWithJson("Responses/offer.json");
        var offer = await server.Offers.Offer("GAHEAZQD6K7QBBPGLQZQOHCA5L3Z7ZNFBDU5XCCEI7QYVEFIRUF3IW27");
        Assert.AreEqual("100.0000000", offer.Amount);
        Assert.AreEqual("1", offer.PagingToken);
        Assert.AreEqual(0, offer.Buying.CompareTo(new AssetTypeNative()));
        Assert.AreEqual("GAHEAZQD6K7QBBPGLQZQOHCA5L3Z7ZNFBDU5XCCEI7QYVEFIRUF3IW27", offer.Seller);
        Assert.AreEqual(0, offer.Selling.CompareTo(Asset.CreateNonNativeAsset("IOM", "GBL37ZFYU3FRBKTFO4YPIAELTO2PN6YEIF3ZS6FKOEAM5SOWALX5IP44")));
        Assert.AreEqual("1", offer.Id);
        Assert.AreEqual("https://horizon-testnet.stellar.org/offers/1", offer.Links.Self.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/accounts/GAHEAZQD6K7QBBPGLQZQOHCA5L3Z7ZNFBDU5XCCEI7QYVEFIRUF3IW27", offer.Links.OfferMaker.Href);
        Assert.AreEqual(new Price(1, 2), offer.PriceRatio);
        Assert.AreEqual("0.5000000", offer.Price);
        Assert.AreEqual(380, offer.LastModifiedLedger);
        Assert.AreEqual("2024-12-10T17:50:08Z", offer.LastModifiedTime);
        
    }
    private async Task<Server> CreateServer()
    {
        return await Utils.CreateTestServerWithJson("Responses/offerPage.json");
    }
}