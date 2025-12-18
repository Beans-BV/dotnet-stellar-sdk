using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
/// Unit tests for <see cref="LedgersRequestBuilder"/> class.
/// </summary>
[TestClass]
public class LedgersRequestBuilderTest
{
    /// <summary>
    /// Verifies that LedgersRequestBuilder.BuildUri correctly constructs URI with limit and order parameters.
    /// </summary>
    [TestMethod]
    public void BuildUri_WithLimitAndOrder_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Ledgers
            .Limit(200)
            .Order(OrderDirection.ASC)
            .BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/ledgers?limit=200&order=asc", uri.ToString());
    }

    /// <summary>
    /// Verifies that LedgersRequestBuilder.Execute correctly retrieves and deserializes ledger page data.
    /// </summary>
    [TestMethod]
    public async Task Execute_WithDefaultParameters_ReturnsDeserializedLedgerPage()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/ledgerPage.json");

        // Act
        var ledgersPage = await server.Ledgers
            .Execute();

        // Assert
        LedgerPageDeserializerTest.AssertTestData(ledgersPage);
    }

    /// <summary>
    /// Verifies that LedgersRequestBuilder.Stream correctly streams and deserializes ledger events.
    /// </summary>
    [TestMethod]
    public async Task Stream_WithValidJson_StreamsLedgerEvents()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("Responses/ledger.json");
        var json = await File.ReadAllTextAsync(jsonPath);

        // Act & Assert
        var streamableTest = new StreamableTest<LedgerResponse>(json, LedgerDeserializeTest.AssertTestData);
        await streamableTest.Run();
    }
}