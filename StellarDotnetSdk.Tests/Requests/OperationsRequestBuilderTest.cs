using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses.Operations;
using StellarDotnetSdk.Tests.Responses.Operations;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
/// Unit tests for <see cref="OperationsRequestBuilder"/> class.
/// </summary>
[TestClass]
public class OperationsRequestBuilderTest
{
    /// <summary>
    /// Verifies that OperationsRequestBuilder.BuildUri correctly constructs URI with limit and order parameters.
    /// </summary>
    [TestMethod]
    public void BuildUri_WithLimitAndOrder_BuildsCorrectUri()
    {
        // Arrange
        using var server = Utils.CreateTestServerWithContent("");

        // Act
        var uri = server.Operations
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/operations?limit=200&order=desc", uri.ToString());
    }

    /// <summary>
    /// Verifies that OperationsRequestBuilder.Operation correctly constructs URI for specific operation ID.
    /// </summary>
    [TestMethod]
    public void Operation_WithValidOperationId_BuildsCorrectUri()
    {
        // Arrange
        using var server = Utils.CreateTestServerWithContent("");

        // Act
        var uri = server.Operations
            .Operation(100000L)
            .BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/operations/100000", uri.ToString());
    }

    /// <summary>
    /// Verifies that OperationsRequestBuilder.ForAccount correctly constructs URI for account operations.
    /// </summary>
    [TestMethod]
    public void ForAccount_WithValidAccountId_BuildsCorrectUri()
    {
        // Arrange
        using var server = Utils.CreateTestServerWithContent("");

        // Act
        var uri = server.Operations
            .ForAccount("GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H")
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H/operations?limit=200&order=desc",
            uri.ToString());
    }

    /// <summary>
    /// Verifies that OperationsRequestBuilder.ForClaimableBalance correctly constructs URI for claimable balance operations with valid ID.
    /// </summary>
    [TestMethod]
    public void ForClaimableBalance_WithValidBalanceId_BuildsCorrectUri()
    {
        // Arrange
        using var server = Utils.CreateTestServerWithContent("");

        // Act
        var uri = server.Operations
            .ForClaimableBalance("00000000846c047755e4a46912336f56096b48ece78ddb5fbf6d90f0eb4ecae5324fbddb")
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/claimable_balances/00000000846c047755e4a46912336f56096b48ece78ddb5fbf6d90f0eb4ecae5324fbddb/operations?limit=200&order=desc",
            uri.ToString());
    }

    /// <summary>
    /// Verifies that OperationsRequestBuilder.ForClaimableBalance throws ArgumentException when balance ID is empty.
    /// </summary>
    /// <param name="invalidId">The invalid balance ID to test.</param>
    [TestMethod]
    [DataRow("")]
    [ExpectedException(typeof(ArgumentException))]
    public void ForClaimableBalance_WithInvalidBalanceId_ThrowsArgumentException(string invalidId)
    {
        // Arrange
        using var server = Utils.CreateTestServerWithContent("");

        // Act & Assert
        _ = server.Operations.ForClaimableBalance(invalidId);
    }

    /// <summary>
    /// Verifies that OperationsRequestBuilder.ForLedger correctly constructs URI for ledger operations.
    /// </summary>
    [TestMethod]
    public void ForLedger_WithValidLedgerId_BuildsCorrectUri()
    {
        // Arrange
        using var server = Utils.CreateTestServerWithContent("");

        // Act
        var uri = server.Operations
            .ForLedger(200000000000L)
            .Limit(50)
            .Order(OrderDirection.ASC)
            .BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/ledgers/200000000000/operations?limit=50&order=asc",
            uri.ToString());
    }

    /// <summary>
    /// Verifies that OperationsRequestBuilder.ForTransaction correctly constructs URI for transaction operations.
    /// </summary>
    [TestMethod]
    public void ForTransaction_WithValidTransactionHash_BuildsCorrectUri()
    {
        // Arrange
        using var server = Utils.CreateTestServerWithContent("");

        // Act
        var uri = server.Operations
            .ForTransaction("991534d902063b7715cd74207bef4e7bd7aa2f108f62d3eba837ce6023b2d4f3")
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/transactions/991534d902063b7715cd74207bef4e7bd7aa2f108f62d3eba837ce6023b2d4f3/operations",
            uri.ToString());
    }

    /// <summary>
    /// Verifies that OperationsRequestBuilder.IncludeFailed correctly adds include_failed parameter to URI.
    /// </summary>
    [TestMethod]
    public void IncludeFailed_WithTrueValue_AddsIncludeFailedParameter()
    {
        // Arrange
        using var server = Utils.CreateTestServerWithContent("");

        // Act
        var uri = server.Operations
            .ForLedger(200000000000L)
            .IncludeFailed(true)
            .Limit(50)
            .Order(OrderDirection.ASC)
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/ledgers/200000000000/operations?include_failed=true&limit=50&order=asc",
            uri.ToString());
    }

    /// <summary>
    /// Verifies that OperationsRequestBuilder.Execute correctly retrieves and deserializes operation page data.
    /// </summary>
    [TestMethod]
    public async Task Execute_ForAccount_ReturnsDeserializedOperationPage()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/operationPage.json");

        // Act
        var account = await server.Operations.ForAccount("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7")
            .Execute();

        // Assert
        OperationPageDeserializerTest.AssertTestData(account);
    }

    /// <summary>
    /// Verifies that OperationsRequestBuilder.Stream correctly streams and deserializes operation events.
    /// </summary>
    [TestMethod]
    public async Task Stream_WithValidJson_StreamsOperationEvents()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("Responses/Operations/createAccount.json");
        var json = await File.ReadAllTextAsync(jsonPath);

        // Act & Assert
        var streamableTest = new StreamableTest<OperationResponse>(json,
            CreateAccountOperationResponseTest.AssertCreateAccountOperationData);
        await streamableTest.Run();
    }
}