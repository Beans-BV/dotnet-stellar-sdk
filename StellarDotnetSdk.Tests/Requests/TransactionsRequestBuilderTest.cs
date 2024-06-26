﻿using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

[TestClass]
public class TransactionsRequestBuilderTest
{
    [TestMethod]
    public void TestTransactions()
    {
        using var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.Transactions
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();
        Assert.AreEqual("https://horizon-testnet.stellar.org/transactions?limit=200&order=desc", uri.ToString());
    }

    [TestMethod]
    public void TestForAccount()
    {
        using var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.Transactions
            .ForAccount("GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H")
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H/transactions?limit=200&order=desc",
            uri.ToString());
    }

    [TestMethod]
    public void TestForClaimableBalance()
    {
        using var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.Transactions
            .ForClaimableBalance("00000000846c047755e4a46912336f56096b48ece78ddb5fbf6d90f0eb4ecae5324fbddb")
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/claimable_balances/00000000846c047755e4a46912336f56096b48ece78ddb5fbf6d90f0eb4ecae5324fbddb/transactions?limit=200&order=desc",
            uri.ToString());
    }

    [TestMethod]
    public void TestForLedger()
    {
        using var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.Transactions
            .ForLedger(200000000000L)
            .Limit(50)
            .Order(OrderDirection.ASC)
            .BuildUri();

        Assert.AreEqual("https://horizon-testnet.stellar.org/ledgers/200000000000/transactions?limit=50&order=asc",
            uri.ToString());
    }

    [TestMethod]
    public void TestIncludeFailed()
    {
        var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.Transactions
            .ForLedger(200000000000L)
            .IncludeFailed(true)
            .Limit(50)
            .Order(OrderDirection.ASC)
            .BuildUri();
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/ledgers/200000000000/transactions?include_failed=true&limit=50&order=asc",
            uri.ToString());
    }

    [TestMethod]
    public async Task TestTransactionsExecute()
    {
        using var server = await Utils.CreateTestServerWithJson("Responses/transactionPage.json");
        var account = await server.Transactions
            .ForAccount("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7")
            .Execute();

        TransactionPageDeserializeTest.AssertTestData(account);
    }

    [TestMethod]
    public async Task TestStream()
    {
        var jsonPath = Utils.GetTestDataPath("Responses/transactionTransaction.json");
        var json = await File.ReadAllTextAsync(jsonPath);

        var streamableTest = new StreamableTest<TransactionResponse>(json, TransactionDeserializerTest.AssertTestData);
        await streamableTest.Run();
    }
}