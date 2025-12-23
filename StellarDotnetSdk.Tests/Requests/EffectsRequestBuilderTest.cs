using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses.Effects;
using StellarDotnetSdk.Tests.Responses.Effects;
using XDR = StellarDotnetSdk.Xdr;

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

    /// <summary>
    ///     Verifies that EffectsRequestBuilder.ForAccount throws ArgumentNullException when account is null.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ForAccount_WithNullAccount_ThrowsArgumentNullException()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act & Assert
        _ = server.Effects.ForAccount(null!);
    }

    /// <summary>
    ///     Verifies that EffectsRequestBuilder.ForAccount throws ArgumentException when account is empty.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void ForAccount_WithEmptyAccount_ThrowsArgumentException()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act & Assert
        _ = server.Effects.ForAccount("");
    }

    /// <summary>
    ///     Verifies that EffectsRequestBuilder.ForAccount throws ArgumentException when account ID is invalid.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void ForAccount_WithInvalidAccountId_ThrowsArgumentException()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act & Assert
        _ = server.Effects.ForAccount("INVALID_ACCOUNT_ID");
    }

    /// <summary>
    ///     Verifies that EffectsRequestBuilder.ForTransaction throws ArgumentNullException when transactionId is null.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ForTransaction_WithNullTransactionId_ThrowsArgumentNullException()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act & Assert
        _ = server.Effects.ForTransaction(null!);
    }

    /// <summary>
    ///     Verifies that EffectsRequestBuilder.ForTransaction throws ArgumentException when transaction ID is empty.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void ForTransaction_WithEmptyTransactionId_ThrowsArgumentException()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act & Assert
        _ = server.Effects.ForTransaction("");
    }

    /// <summary>
    ///     Verifies that EffectsRequestBuilder.ForLiquidityPool correctly constructs URI for liquidity pool effects with LiquidityPoolId.
    /// </summary>
    [TestMethod]
    public void ForLiquidityPool_WithLiquidityPoolId_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");
        var assetA = Asset.Create("native");
        var assetB = Asset.CreateNonNativeAsset("ABC", "GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3");
        var liquidityPoolId = new LiquidityPoolId(
            XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, assetA, assetB,
            LiquidityPoolParameters.Fee);

        // Act
        var uri = server.Effects
            .ForLiquidityPool(liquidityPoolId)
            .BuildUri();

        // Assert
        Assert.AreEqual(
            $"https://horizon-testnet.stellar.org/liquidity_pools/{liquidityPoolId}/effects",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that EffectsRequestBuilder.ForLiquidityPool correctly constructs URI for liquidity pool effects with string ID.
    /// </summary>
    [TestMethod]
    public void ForLiquidityPool_WithStringId_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");
        const string poolId = "cc22414997d7e3d9a9ac3b1d65ca9cc3e5f35ce33e0bd6a885648b11aaa3b72d";

        // Act
        var uri = server.Effects
            .ForLiquidityPool(poolId)
            .Limit(50)
            .Order(OrderDirection.ASC)
            .BuildUri();

        // Assert
        Assert.AreEqual(
            $"https://horizon-testnet.stellar.org/liquidity_pools/{poolId}/effects?limit=50&order=asc",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that EffectsRequestBuilder.ForLiquidityPool throws ArgumentNullException when pool ID is null.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ForLiquidityPool_WithNullPoolId_ThrowsArgumentNullException()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act & Assert
        _ = server.Effects.ForLiquidityPool((string)null!);
    }

    /// <summary>
    ///     Verifies that EffectsRequestBuilder.ForLiquidityPool throws ArgumentException when pool ID is empty.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void ForLiquidityPool_WithEmptyPoolId_ThrowsArgumentException()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act & Assert
        _ = server.Effects.ForLiquidityPool("");
    }
}