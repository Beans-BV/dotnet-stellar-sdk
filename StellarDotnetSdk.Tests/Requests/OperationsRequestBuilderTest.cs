using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses.Operations;
using StellarDotnetSdk.Tests.Responses.Operations;

namespace StellarDotnetSdk.Tests.Requests;

[TestClass]
public class OperationsRequestBuilderTest
{
    [TestMethod]
    public void TestOperations()
    {
        using var server = Utils.CreateTestServerWithContent("");
        var uri = server.Operations
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();
        Assert.AreEqual("https://horizon-testnet.stellar.org/operations?limit=200&order=desc", uri.ToString());
    }

    [TestMethod]
    public void TestForOperation()
    {
        using var server = Utils.CreateTestServerWithContent("");
        var uri = server.Operations
            .Operation(100000L)
            .BuildUri();
        Assert.AreEqual("https://horizon-testnet.stellar.org/operations/100000", uri.ToString());
    }

    [TestMethod]
    public void TestForAccount()
    {
        using var server = Utils.CreateTestServerWithContent("");
        var uri = server.Operations
            .ForAccount("GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H")
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H/operations?limit=200&order=desc",
            uri.ToString());
    }

    [TestMethod]
    public void TestForClaimableBalanceWithValidArgument()
    {
        using var server = Utils.CreateTestServerWithContent("");
        var uri = server.Operations
            .ForClaimableBalance("00000000846c047755e4a46912336f56096b48ece78ddb5fbf6d90f0eb4ecae5324fbddb")
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/claimable_balances/00000000846c047755e4a46912336f56096b48ece78ddb5fbf6d90f0eb4ecae5324fbddb/operations?limit=200&order=desc",
            uri.ToString());
    }

    [TestMethod]
    [DataRow("")]
    [ExpectedException(typeof(ArgumentException))]
    public void TestForClaimableBalanceWithInvalidArgument(string invalidId)
    {
        using var server = Utils.CreateTestServerWithContent("");
        _ = server.Operations.ForClaimableBalance(invalidId);
    }

    [TestMethod]
    public void TestLedger()
    {
        using var server = Utils.CreateTestServerWithContent("");
        var uri = server.Operations
            .ForLedger(200000000000L)
            .Limit(50)
            .Order(OrderDirection.ASC)
            .BuildUri();
        Assert.AreEqual("https://horizon-testnet.stellar.org/ledgers/200000000000/operations?limit=50&order=asc",
            uri.ToString());
    }

    [TestMethod]
    public void TestTransaction()
    {
        using var server = Utils.CreateTestServerWithContent("");
        var uri = server.Operations
            .ForTransaction("991534d902063b7715cd74207bef4e7bd7aa2f108f62d3eba837ce6023b2d4f3")
            .BuildUri();
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/transactions/991534d902063b7715cd74207bef4e7bd7aa2f108f62d3eba837ce6023b2d4f3/operations",
            uri.ToString());
    }

    [TestMethod]
    public void TestIncludeFailed()
    {
        using var server = Utils.CreateTestServerWithContent("");
        var uri = server.Operations
            .ForLedger(200000000000L)
            .IncludeFailed(true)
            .Limit(50)
            .Order(OrderDirection.ASC)
            .BuildUri();
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/ledgers/200000000000/operations?include_failed=true&limit=50&order=asc",
            uri.ToString());
    }

    [TestMethod]
    public async Task TestOperationsExecute()
    {
        using var server = await Utils.CreateTestServerWithJson("Responses/operationPage.json");
        var account = await server.Operations.ForAccount("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7")
            .Execute();

        OperationPageDeserializerTest.AssertTestData(account);
    }

    [TestMethod]
    public async Task TestStream()
    {
        var jsonPath = Utils.GetTestDataPath("Responses/Operations/createAccount.json");
        var json = await File.ReadAllTextAsync(jsonPath);

        var streamableTest = new StreamableTest<OperationResponse>(json,
            CreateAccountOperationResponseTest.AssertCreateAccountOperationData);
        await streamableTest.Run();
    }
}