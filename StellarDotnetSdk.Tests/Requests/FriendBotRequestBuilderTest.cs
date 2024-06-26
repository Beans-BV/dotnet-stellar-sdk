﻿using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

[TestClass]
public class FriendBotRequestBuilderTest
{
    [TestCleanup]
    public void Cleanup()
    {
        Network.Use(null);
    }

    [TestMethod]
    [ExpectedException(typeof(NotSupportedException))]
    public void TestThrowExceptionIfNotTestnetNetwork()
    {
        Network.UsePublicNetwork();

        using var server = new Server("https://horizon.stellar.org");
        var unused = server.TestNetFriendBot;
    }

    [TestMethod]
    public void TestDoNotThrowExceptionIfTestnetNetwork()
    {
        Network.UseTestNetwork();

        using var server = new Server("https://horizon-testnet.stellar.org");
        var unused = server.TestNetFriendBot;
    }

    [TestMethod]
    public void TestFund()
    {
        Network.UseTestNetwork();
        using var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.TestNetFriendBot
            .FundAccount("GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H")
            .BuildUri();

        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/friendbot?addr=GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H",
            uri.ToString());
    }

    [TestMethod]
    public async Task TestFriendBotExecute()
    {
        using var server = await Utils.CreateTestServerWithJson("Responses/friendBotSuccess.json");
        var friendBotResponse = await server.TestNetFriendBot
            .FundAccount("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7")
            .Execute();

        FriendBotResponseTest.AssertSuccessTestData(friendBotResponse);
    }
}