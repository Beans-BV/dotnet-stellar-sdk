using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses.Effects;
using StellarDotnetSdk.Tests.Responses.Effects;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
///     Unit tests for <see cref="EffectsRequestBuilder" /> class.
/// </summary>
[TestClass]
public class EffectsRequestBuilderTest
{
    /// <summary>
    ///     Verifies that EffectsRequestBuilder.BuildUri correctly constructs URI with limit and order parameters.
    /// </summary>
    [TestMethod]
    public void BuildUri_WithLimitAndOrder_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Effects
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/effects?limit=200&order=desc", uri.ToString());
    }

    /// <summary>
    ///     Verifies that EffectsRequestBuilder.ForAccount correctly constructs URI for account effects.
    /// </summary>
    [TestMethod]
    public void ForAccount_WithValidAccountId_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Effects
            .ForAccount("GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H")
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H/effects?limit=200&order=desc",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that EffectsRequestBuilder.ForLedger correctly constructs URI for ledger effects.
    /// </summary>
    [TestMethod]
    public void ForLedger_WithValidLedgerId_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Effects
            .ForLedger(200000000000L)
            .Limit(50)
            .Order(OrderDirection.ASC)
            .BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/ledgers/200000000000/effects?limit=50&order=asc",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that EffectsRequestBuilder.ForTransaction correctly constructs URI for transaction effects.
    /// </summary>
    [TestMethod]
    public void ForTransaction_WithValidTransactionHash_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Effects
            .ForTransaction("991534d902063b7715cd74207bef4e7bd7aa2f108f62d3eba837ce6023b2d4f3")
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/transactions/991534d902063b7715cd74207bef4e7bd7aa2f108f62d3eba837ce6023b2d4f3/effects",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that EffectsRequestBuilder.ForOperation correctly constructs URI for operation effects with cursor.
    /// </summary>
    [TestMethod]
    public void ForOperation_WithValidOperationIdAndCursor_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Effects
            .ForOperation(28798257847L)
            .Cursor("85794837")
            .BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/operations/28798257847/effects?cursor=85794837",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that EffectsRequestBuilder.Execute correctly retrieves and deserializes effects page data.
    /// </summary>
    [TestMethod]
    public async Task Execute_ForAccount_ReturnsDeserializedEffectsPage()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/Effects/effectPage.json");

        // Act
        var effectsPage = await server.Effects
            .ForAccount("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7")
            .Execute();

        // Assert
        EffectsPageDeserializerTest.AssertTestData(effectsPage);
    }

    /// <summary>
    ///     Verifies that EffectsRequestBuilder.Stream correctly streams and deserializes effect events.
    /// </summary>
    [TestMethod]
    public async Task Stream_WithValidJson_StreamsEffectEvents()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("Responses/Effects/effectAccountCreated.json");
        var json = await File.ReadAllTextAsync(jsonPath);

        // Act & Assert
        var streamableTest = new StreamableTest<EffectResponse>(json, EffectDeserializerTest.AssertAccountCreatedData);
        await streamableTest.Run();
    }

    /// <summary>
    ///     Verifies that EffectsRequestBuilder.Stream correctly sets cursor and event ID when streaming with cursor.
    /// </summary>
    [TestMethod]
    public async Task Stream_WithCursor_SetsLastEventIdAndCursorInUri()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("Responses/Effects/effectAccountCreated.json");
        var json = await File.ReadAllTextAsync(jsonPath);
        const string eventId = "65571265847297-1";

        // Act
        var streamableTest =
            new StreamableTest<EffectResponse>(json, EffectDeserializerTest.AssertAccountCreatedData, eventId);
        await streamableTest.Run();

        // Assert
        Assert.AreEqual(eventId, streamableTest.LastEventId);
        Assert.AreEqual("https://horizon-testnet.stellar.org/test?cursor=65571265847297-1", streamableTest.Uri);
    }
}