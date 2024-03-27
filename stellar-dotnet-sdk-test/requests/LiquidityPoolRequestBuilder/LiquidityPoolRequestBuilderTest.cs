﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using stellar_dotnet_sdk;
using System;
using System.Collections.Generic;
using System.Text;
using stellar_dotnet_sdk.requests;
using System.Threading.Tasks;
using System.IO;
using stellar_dotnet_sdk.responses;
using stellar_dotnet_sdk_test.responses;

namespace stellar_dotnet_sdk_test.requests
{
    [TestClass]
    public class LiquidityPoolRequestBuilderTest
    {
        [TestMethod]
        public void TestLiquidityPools()
        {
            using (var server = new Server("https://horizon-testnet.stellar.org"))
            {
                var uri = server.LiquidityPools
                        .Cursor("13537736921089")
                        .Limit(200)
                        .Order(OrderDirection.ASC)
                        .BuildUri();
                Assert.AreEqual("https://horizon-testnet.stellar.org/liquidity_pools?cursor=13537736921089&limit=200&order=asc", uri.ToString());
            }
        }

        [TestMethod]
        public void TestForReserves()
        {
            using (var server = new Server("https://horizon-testnet.stellar.org"))
            {
                var uri = server.LiquidityPools
                        .ForReserves("EURT:GAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S", "PHP:GAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S")
                        .BuildUri();
                Assert.AreEqual("https://horizon-testnet.stellar.org/liquidity_pools?reserves=EURT%3aGAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S%2cPHP%3aGAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S", uri.ToString());
            }
        }

        [TestMethod]
        public async Task TestLiquidityPoolExecute()
        {
            var json = File.ReadAllText(Path.Combine("requests/LiquidityPoolRequestBuilder", "DataPage.json"));
            var fakeHttpClient = FakeHttpClient.CreateFakeHttpClient(json);

            using (var server = new Server("https://horizon-testnet.stellar.org", fakeHttpClient))
            {
                var pages = await server.LiquidityPools.Execute();

                LiquidityPoolPageDeserializerTest.AssertTestData(pages);
            }
        }

        [TestMethod]
        public async Task TestStream()
        {
            var json = File.ReadAllText(Path.Combine("requests/LiquidityPoolRequestBuilder", "Data.json"));

            var streamableTest = new StreamableTest<LiquidityPoolResponse>(json, LiquidityPoolDeserializerTest.AssertTestData);
            await streamableTest.Run();
        }
    }
}
