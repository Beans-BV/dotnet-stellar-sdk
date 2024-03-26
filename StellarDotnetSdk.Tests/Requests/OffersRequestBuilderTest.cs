using System.IO;
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
        using (var server = new Server("https://horizon-testnet.stellar.org"))
        {
            var uri = server.Offers
                .ForAccount("GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H")
                .Limit(200)
                .Order(OrderDirection.DESC)
                .BuildUri();
            Assert.AreEqual(
                "https://horizon-testnet.stellar.org/accounts/GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H/offers?limit=200&order=desc",
                uri.ToString());
        }
    }

    [TestMethod]
    public async Task TestOffersExecute()
    {
        var jsonResponse = await File.ReadAllTextAsync(Path.Combine("testdata", "offerPage.json"));
        var fakeHttpClient = FakeHttpClient.CreateFakeHttpClient(jsonResponse);

        using (var server = new Server("https://horizon-testnet.stellar.org", fakeHttpClient))
        {
            var offerResponsePage = await server.Offers
                .ForAccount("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7")
                .Execute();

            OfferPageDeserializerTest.AssertTestData(offerResponsePage);
        }
    }

    [TestMethod]
    public async Task TestOffersWithSeller()
    {
        var jsonResponse = await File.ReadAllTextAsync(Path.Combine("testdata", "offerPage.json"));
        var fakeHttpClient = FakeHttpClient.CreateFakeHttpClient(jsonResponse);

        using (var server = new Server("https://horizon-testnet.stellar.org", fakeHttpClient))
        {
            var req = server.Offers.WithSeller("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7");

            Assert.AreEqual(
                "https://horizon-testnet.stellar.org/offers?seller=GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7",
                req.BuildUri().ToString());

            var offerResponsePage = await req.Execute();
            OfferPageDeserializerTest.AssertTestData(offerResponsePage);
        }
    }

    [TestMethod]
    public async Task TestOffersWithSellingNativeAsset()
    {
        var jsonResponse = await File.ReadAllTextAsync(Path.Combine("testdata", "offerPage.json"));
        var fakeHttpClient = FakeHttpClient.CreateFakeHttpClient(jsonResponse);

        using (var server = new Server("https://horizon-testnet.stellar.org", fakeHttpClient))
        {
            var req = server.Offers.WithSellingAsset(new AssetTypeNative());

            Assert.AreEqual(
                "https://horizon-testnet.stellar.org/offers?selling_asset_type=native",
                req.BuildUri().ToString());

            var offerResponsePage = await req.Execute();
            OfferPageDeserializerTest.AssertTestData(offerResponsePage);
        }
    }

    [TestMethod]
    public async Task TestOffersWithSellingCreditAsset()
    {
        var jsonResponse = await File.ReadAllTextAsync(Path.Combine("testdata", "offerPage.json"));
        var fakeHttpClient = FakeHttpClient.CreateFakeHttpClient(jsonResponse);

        using (var server = new Server("https://horizon-testnet.stellar.org", fakeHttpClient))
        {
            var nonNativeAsset =
                Asset.CreateNonNativeAsset("FOO", "GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7");
            var req = server.Offers.WithSellingAsset(nonNativeAsset);

            Assert.AreEqual(
                "https://horizon-testnet.stellar.org/offers?selling_asset_type=credit_alphanum4&selling_asset_code=FOO&selling_asset_issuer=GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7",
                req.BuildUri().ToString());

            var offerResponsePage = await req.Execute();
            OfferPageDeserializerTest.AssertTestData(offerResponsePage);
        }
    }

    [TestMethod]
    public async Task TestOffersWithBuyingNativeAsset()
    {
        var jsonResponse = await File.ReadAllTextAsync(Path.Combine("testdata", "offerPage.json"));
        var fakeHttpClient = FakeHttpClient.CreateFakeHttpClient(jsonResponse);

        using (var server = new Server("https://horizon-testnet.stellar.org", fakeHttpClient))
        {
            var req = server.Offers.WithBuyingAsset(new AssetTypeNative());

            Assert.AreEqual(
                "https://horizon-testnet.stellar.org/offers?buying_asset_type=native",
                req.BuildUri().ToString());

            var offerResponsePage = await req.Execute();
            OfferPageDeserializerTest.AssertTestData(offerResponsePage);
        }
    }

    [TestMethod]
    public async Task TestOffersWithBuyingCreditAsset()
    {
        var jsonResponse = await File.ReadAllTextAsync(Path.Combine("testdata", "offerPage.json"));
        var fakeHttpClient = FakeHttpClient.CreateFakeHttpClient(jsonResponse);

        using (var server = new Server("https://horizon-testnet.stellar.org", fakeHttpClient))
        {
            var notNativeAsset = Asset.CreateNonNativeAsset("FOOBARBAZ",
                "GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7");
            var req = server.Offers.WithBuyingAsset(notNativeAsset);

            Assert.AreEqual(
                "https://horizon-testnet.stellar.org/offers?buying_asset_type=credit_alphanum12&buying_asset_code=FOOBARBAZ&buying_asset_issuer=GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7",
                req.BuildUri().ToString());

            var offerResponsePage = await req.Execute();
            OfferPageDeserializerTest.AssertTestData(offerResponsePage);
        }
    }
}