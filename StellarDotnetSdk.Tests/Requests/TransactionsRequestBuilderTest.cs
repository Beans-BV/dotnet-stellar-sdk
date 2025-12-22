using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
///     Unit tests for <see cref="TransactionsRequestBuilder" /> class.
/// </summary>
[TestClass]
public class TransactionsRequestBuilderTest
{
    /// <summary>
    ///     Verifies that TransactionsRequestBuilder.BuildUri correctly constructs URI with limit and order parameters.
    /// </summary>
    [TestMethod]
    public void BuildUri_WithLimitAndOrder_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Transactions
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/transactions?limit=200&order=desc", uri.ToString());
    }

    /// <summary>
    ///     Verifies that TransactionsRequestBuilder.ForAccount correctly constructs URI for account transactions.
    /// </summary>
    [TestMethod]
    public void ForAccount_WithValidAccountId_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Transactions
            .ForAccount("GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H")
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H/transactions?limit=200&order=desc",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that TransactionsRequestBuilder.ForClaimableBalance correctly constructs URI for claimable balance
    ///     transactions.
    /// </summary>
    [TestMethod]
    public void ForClaimableBalance_WithValidBalanceId_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Transactions
            .ForClaimableBalance("00000000846c047755e4a46912336f56096b48ece78ddb5fbf6d90f0eb4ecae5324fbddb")
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/claimable_balances/00000000846c047755e4a46912336f56096b48ece78ddb5fbf6d90f0eb4ecae5324fbddb/transactions?limit=200&order=desc",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that TransactionsRequestBuilder.ForLedger correctly constructs URI for ledger transactions.
    /// </summary>
    [TestMethod]
    public void ForLedger_WithValidLedgerId_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Transactions
            .ForLedger(200000000000L)
            .Limit(50)
            .Order(OrderDirection.ASC)
            .BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/ledgers/200000000000/transactions?limit=50&order=asc",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that TransactionsRequestBuilder.IncludeFailed correctly adds include_failed parameter to URI.
    /// </summary>
    [TestMethod]
    public void IncludeFailed_WithTrueValue_AddsIncludeFailedParameter()
    {
        // Arrange
        var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Transactions
            .ForLedger(200000000000L)
            .IncludeFailed(true)
            .Limit(50)
            .Order(OrderDirection.ASC)
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/ledgers/200000000000/transactions?include_failed=true&limit=50&order=asc",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that TransactionsRequestBuilder.Execute correctly retrieves and deserializes transaction page data.
    /// </summary>
    [TestMethod]
    public async Task Execute_ForAccount_ReturnsDeserializedTransactionPage()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/transactionPage.json");

        // Act
        var account = await server.Transactions
            .ForAccount("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7")
            .Execute();

        // Assert
        TransactionPageDeserializeTest.AssertTestData(account);
    }

    /// <summary>
    ///     Verifies that TransactionsRequestBuilder.Stream correctly streams and deserializes transaction events.
    /// </summary>
    [TestMethod]
    public async Task Stream_WithValidJson_StreamsTransactionEvents()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("Responses/transaction.json");
        var json = await File.ReadAllTextAsync(jsonPath);

        // Act & Assert
        var streamableTest =
            new StreamableTest<TransactionResponse>(json, TransactionDeserializerTest.AssertTransaction);
        await streamableTest.Run();
    }

    /// <summary>
    ///     Verifies that TransactionsRequestBuilder.Transaction correctly retrieves and deserializes transaction data from URI.
    /// </summary>
    [TestMethod]
    public async Task Transaction_WithValidUri_ReturnsDeserializedTransaction()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/transaction.json");
        var uri = new Uri("https://horizon-testnet.stellar.org/transactions/991534d902063b7715cd74207bef4e7bd7aa2f108f62d3eba837ce6023b2d4f3");

        // Act
        var transaction = await server.Transactions.Transaction(uri);

        // Assert
        TransactionDeserializerTest.AssertTransaction(transaction);
    }

    /// <summary>
    ///     Verifies that TransactionsRequestBuilder.Transaction correctly retrieves and deserializes transaction data from transaction ID.
    /// </summary>
    [TestMethod]
    public async Task Transaction_WithValidTransactionId_ReturnsDeserializedTransaction()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/transaction.json");
        const string transactionId = "991534d902063b7715cd74207bef4e7bd7aa2f108f62d3eba837ce6023b2d4f3";

        // Act
        var transaction = await server.Transactions.Transaction(transactionId);

        // Assert
        TransactionDeserializerTest.AssertTransaction(transaction);
    }

    /// <summary>
    ///     Verifies that TransactionsRequestBuilder.ForAccount throws ArgumentNullException when account is null.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ForAccount_WithNullAccount_ThrowsArgumentNullException()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act & Assert
        _ = server.Transactions.ForAccount(null!);
    }

    /// <summary>
    ///     Verifies that TransactionsRequestBuilder.ForClaimableBalance throws ArgumentNullException when claimableBalance is null.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ForClaimableBalance_WithNullBalanceId_ThrowsArgumentNullException()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act & Assert
        _ = server.Transactions.ForClaimableBalance(null!);
    }

    /// <summary>
    ///     Verifies that TransactionsRequestBuilder.ForClaimableBalance throws ArgumentNullException when claimableBalance is whitespace.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ForClaimableBalance_WithWhitespaceBalanceId_ThrowsArgumentNullException()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act & Assert
        _ = server.Transactions.ForClaimableBalance("   ");
    }

    /// <summary>
    ///     Verifies that TransactionsRequestBuilder.IncludeFailed correctly adds include_failed parameter with false value.
    /// </summary>
    [TestMethod]
    public void IncludeFailed_WithFalseValue_AddsIncludeFailedParameter()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Transactions
            .ForLedger(200000000000L)
            .IncludeFailed(false)
            .Limit(50)
            .Order(OrderDirection.ASC)
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/ledgers/200000000000/transactions?include_failed=false&limit=50&order=asc",
            uri.ToString());
    }
}