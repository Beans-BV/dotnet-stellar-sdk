﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using stellar_dotnetcore_sdk;
using stellar_dotnetcore_sdk.responses;
using stellar_dotnetcore_sdk.responses.accountResponse;
using stellar_dotnetcore_sdk.responses.page;

namespace stellar_dotnetcore_unittest.responses
{
    [TestClass]
    public class OfferPageDeserializerTest
    {
        [TestMethod]
        public void TestDeserialize()
        {
            var json = File.ReadAllText(Path.Combine("responses", "testdata", "offerPage.json"));
            var transactionPage = JsonSingleton.GetInstance<Page<OfferResponse>>(json);

            Assert.AreEqual(transactionPage.Records[0].Id, 241);
            Assert.AreEqual(transactionPage.Records[0].Seller.AccountId, "GA2IYMIZSAMDD6QQTTSIEL73H2BKDJQTA7ENDEEAHJ3LMVF7OYIZPXQD");
            Assert.AreEqual(transactionPage.Records[0].PagingToken, "241");
            Assert.AreEqual(transactionPage.Records[0].Selling, Asset.CreateNonNativeAsset("INR", KeyPair.FromAccountId("GA2IYMIZSAMDD6QQTTSIEL73H2BKDJQTA7ENDEEAHJ3LMVF7OYIZPXQD")));
            Assert.AreEqual(transactionPage.Records[0].Buying, Asset.CreateNonNativeAsset("USD", KeyPair.FromAccountId("GA2IYMIZSAMDD6QQTTSIEL73H2BKDJQTA7ENDEEAHJ3LMVF7OYIZPXQD")));
            Assert.AreEqual(transactionPage.Records[0].Amount, "10.0000000");
            Assert.AreEqual(transactionPage.Records[0].Price, "11.0000000");

            Assert.AreEqual(transactionPage.Links.Next.Href, "https://horizon-testnet.stellar.org/accounts/GA2IYMIZSAMDD6QQTTSIEL73H2BKDJQTA7ENDEEAHJ3LMVF7OYIZPXQD/offers?order=asc&limit=10&cursor=241");
            Assert.AreEqual(transactionPage.Links.Prev.Href, "https://horizon-testnet.stellar.org/accounts/GA2IYMIZSAMDD6QQTTSIEL73H2BKDJQTA7ENDEEAHJ3LMVF7OYIZPXQD/offers?order=desc&limit=10&cursor=241");
            Assert.AreEqual(transactionPage.Links.Self.Href, "https://horizon-testnet.stellar.org/accounts/GA2IYMIZSAMDD6QQTTSIEL73H2BKDJQTA7ENDEEAHJ3LMVF7OYIZPXQD/offers?order=asc&limit=10&cursor=");
        }


    }
}
