﻿using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses.Operations;
using StellarDotnetSdk.Tests.Responses.Operations;

namespace StellarDotnetSdk.Tests.Requests;

[TestClass]
public class PaymentsRequestBuilderTest
{
    [TestMethod]
    public void TestPayments()
    {
        using var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.Payments
            .IncludeTransaction()
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();
        Assert.AreEqual("https://horizon-testnet.stellar.org/payments?join=transactions&limit=200&order=desc",
            uri.ToString());
    }

    [TestMethod]
    public void TestForAccount()
    {
        using var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.Payments
            .ForAccount("GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H")
            .IncludeTransaction()
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H/payments?join=transactions&limit=200&order=desc",
            uri.ToString());
    }

    [TestMethod]
    public void TestForLedger()
    {
        using var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.Payments
            .ForLedger(200000000000L)
            .IncludeTransaction()
            .Limit(50)
            .Order(OrderDirection.ASC)
            .BuildUri();
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/ledgers/200000000000/payments?join=transactions&limit=50&order=asc",
            uri.ToString());
    }

    [TestMethod]
    public void TestForTransaction()
    {
        using var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.Payments
            .ForTransaction("991534d902063b7715cd74207bef4e7bd7aa2f108f62d3eba837ce6023b2d4f3")
            .BuildUri();
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/transactions/991534d902063b7715cd74207bef4e7bd7aa2f108f62d3eba837ce6023b2d4f3/payments",
            uri.ToString());
    }

    [TestMethod]
    public async Task TestPaymentsExecute()
    {
        using var server = await Utils.CreateTestServerWithJson("Responses/operationPage.json");
        var payments = await server.Payments
            .ForAccount("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7")
            .Execute();

        OperationPageDeserializerTest.AssertTestData(payments);
    }

    [TestMethod]
    public async Task TestStream()
    {
        var jsonPath = Utils.GetTestDataPath("Responses/Operations/payment.json");
        var json = await File.ReadAllTextAsync(jsonPath);

        var streamableTest =
            new StreamableTest<PaymentOperationResponse>(json,
                PaymentOperationResponseTest.AssertPaymentOperationTestData);
        await streamableTest.Run();
    }
}