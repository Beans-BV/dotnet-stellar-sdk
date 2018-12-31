﻿using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using stellar_dotnet_sdk.responses;
using stellar_dotnet_sdk.responses.page;

namespace stellar_dotnet_sdk_test.responses
{
    [TestClass]
    public class AccountPageDeserializerTest
    {
        [TestMethod]
        public void TestDeserializeAccountPage()
        {
            var json = File.ReadAllText(Path.Combine("testdata", "accountPage.json"));
            var accountsPage = JsonSingleton.GetInstance<Page<AccountResponse>>(json);
            AssertTestData(accountsPage);
        }

        [TestMethod]
        public void TestSerializeDeserializeAccountPage()
        {
            var json = File.ReadAllText(Path.Combine("testdata", "accountPage.json"));
            var accountsPage = JsonSingleton.GetInstance<Page<AccountResponse>>(json);
            var serialized = JsonConvert.SerializeObject(accountsPage);
            var back = JsonConvert.DeserializeObject<Page<AccountResponse>>(serialized);
            AssertTestData(back);
        }

        public static void AssertTestData(Page<AccountResponse> accountsPage)
        {
            Assert.AreEqual(accountsPage.Records[0].AccountId, "GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7");
            Assert.AreEqual(accountsPage.Records[0].PagingToken, "1");
            Assert.AreEqual(accountsPage.Records[9].AccountId, "GACFGMEV7A5H44O3K4EN6GRQ4SA543YJBZTKGNKPEMEQEAJFO4Q7ENG6");
            Assert.AreEqual(accountsPage.Links.Next.Href, "/accounts?order=asc&limit=10&cursor=86861418598401");
            Assert.AreEqual(accountsPage.Links.Prev.Href, "/accounts?order=desc&limit=10&cursor=1");
            Assert.AreEqual(accountsPage.Links.Self.Href, "/accounts?order=asc&limit=10&cursor=");
        }
    }
}
