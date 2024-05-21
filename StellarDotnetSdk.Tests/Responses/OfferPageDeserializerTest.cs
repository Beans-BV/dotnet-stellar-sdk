using System.IO;
using System.Text.Json;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class OfferPageDeserializerTest
{
    [TestMethod]
    public void TestDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("offerPage.json");
        var json = File.ReadAllText(jsonPath);
        var offerResponsePage = JsonSerializer.Deserialize<Page<OfferResponse>>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(offerResponsePage);
        AssertTestData(offerResponsePage);
    }

    [TestMethod]
    public void TestSerializeDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("offerPage.json");
        var json = File.ReadAllText(jsonPath);
        var offerResponsePage = JsonSerializer.Deserialize<Page<OfferResponse>>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(offerResponsePage);
        var back = JsonSerializer.Deserialize<Page<OfferResponse>>(serialized);
        Assert.IsNotNull(back);
        AssertTestData(back);
    }

    public static void AssertTestData(Page<OfferResponse> offerResponsePage)
    {
        Assert.AreEqual(offerResponsePage.Records[0].Id, "241");
        Assert.AreEqual(offerResponsePage.Records[0].Seller,
            "GA2IYMIZSAMDD6QQTTSIEL73H2BKDJQTA7ENDEEAHJ3LMVF7OYIZPXQD");
        Assert.AreEqual(offerResponsePage.Records[0].PagingToken, "241");
        Assert.AreEqual(offerResponsePage.Records[0].Selling,
            Asset.CreateNonNativeAsset("INR", "GA2IYMIZSAMDD6QQTTSIEL73H2BKDJQTA7ENDEEAHJ3LMVF7OYIZPXQD"));
        Assert.AreEqual(offerResponsePage.Records[0].Buying,
            Asset.CreateNonNativeAsset("USD", "GA2IYMIZSAMDD6QQTTSIEL73H2BKDJQTA7ENDEEAHJ3LMVF7OYIZPXQD"));
        Assert.AreEqual(offerResponsePage.Records[0].Amount, "10.0000000");

        offerResponsePage.Records[0].Price
            .Should().Be("1.1000000");

        offerResponsePage.Records[0].PriceRatio.Numerator
            .Should().Be(11);

        offerResponsePage.Records[0].PriceRatio.Denominator
            .Should().Be(10);

        Assert.AreEqual(offerResponsePage.Records[0].LastModifiedLedger, 22200794);
        Assert.AreEqual(offerResponsePage.Records[0].LastModifiedTime, "2019-01-28T12:30:38Z");

        Assert.AreEqual(offerResponsePage.Links.Next.Href,
            "https://horizon-testnet.stellar.org/accounts/GA2IYMIZSAMDD6QQTTSIEL73H2BKDJQTA7ENDEEAHJ3LMVF7OYIZPXQD/offers?order=asc&limit=10&cursor=241");
        Assert.AreEqual(offerResponsePage.Links.Prev.Href,
            "https://horizon-testnet.stellar.org/accounts/GA2IYMIZSAMDD6QQTTSIEL73H2BKDJQTA7ENDEEAHJ3LMVF7OYIZPXQD/offers?order=desc&limit=10&cursor=241");
        Assert.AreEqual(offerResponsePage.Links.Self.Href,
            "https://horizon-testnet.stellar.org/accounts/GA2IYMIZSAMDD6QQTTSIEL73H2BKDJQTA7ENDEEAHJ3LMVF7OYIZPXQD/offers?order=asc&limit=10&cursor=");
    }
}