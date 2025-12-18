using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses.Operations;
using StellarDotnetSdk.Tests.Responses.Operations;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
///     Unit tests for <see cref="PaymentsRequestBuilder" /> class.
/// </summary>
[TestClass]
public class PaymentsRequestBuilderTest
{
    /// <summary>
    ///     Verifies that PaymentsRequestBuilder.BuildUri correctly constructs URI with include transaction, limit, and order
    ///     parameters.
    /// </summary>
    [TestMethod]
    public void BuildUri_WithIncludeTransactionLimitAndOrder_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Payments
            .IncludeTransaction()
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/payments?join=transactions&limit=200&order=desc",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that PaymentsRequestBuilder.ForAccount correctly constructs URI for account payments.
    /// </summary>
    [TestMethod]
    public void ForAccount_WithValidAccountId_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Payments
            .ForAccount("GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H")
            .IncludeTransaction()
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H/payments?join=transactions&limit=200&order=desc",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that PaymentsRequestBuilder.ForLedger correctly constructs URI for ledger payments.
    /// </summary>
    [TestMethod]
    public void ForLedger_WithValidLedgerId_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Payments
            .ForLedger(200000000000L)
            .IncludeTransaction()
            .Limit(50)
            .Order(OrderDirection.ASC)
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/ledgers/200000000000/payments?join=transactions&limit=50&order=asc",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that PaymentsRequestBuilder.ForTransaction correctly constructs URI for transaction payments.
    /// </summary>
    [TestMethod]
    public void ForTransaction_WithValidTransactionHash_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Payments
            .ForTransaction("991534d902063b7715cd74207bef4e7bd7aa2f108f62d3eba837ce6023b2d4f3")
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/transactions/991534d902063b7715cd74207bef4e7bd7aa2f108f62d3eba837ce6023b2d4f3/payments",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that PaymentsRequestBuilder.Execute correctly retrieves and deserializes payment page data.
    /// </summary>
    [TestMethod]
    public async Task Execute_ForAccount_ReturnsDeserializedOperationPage()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/operationPage.json");

        // Act
        var payments = await server.Payments
            .ForAccount("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7")
            .Execute();

        // Assert
        OperationPageDeserializerTest.AssertTestData(payments);
    }

    /// <summary>
    ///     Verifies that PaymentsRequestBuilder.Stream correctly streams and deserializes payment events.
    /// </summary>
    [TestMethod]
    public async Task Stream_WithValidJson_StreamsPaymentEvents()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("Responses/Operations/payment.json");
        var json = await File.ReadAllTextAsync(jsonPath);

        // Act & Assert
        var streamableTest =
            new StreamableTest<PaymentOperationResponse>(json,
                PaymentOperationResponseTest.AssertPaymentOperationTestData);
        await streamableTest.Run();
    }
}