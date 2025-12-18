using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
///     Unit tests for <see cref="LiquidityPoolRequestBuilder" /> class.
/// </summary>
[TestClass]
public class LiquidityPoolRequestBuilderTest
{
    /// <summary>
    ///     Verifies that LiquidityPoolRequestBuilder.BuildUri correctly constructs URI with cursor, limit, and order
    ///     parameters.
    /// </summary>
    [TestMethod]
    public void BuildUri_WithCursorLimitAndOrder_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.LiquidityPools
            .Cursor("13537736921089")
            .Limit(200)
            .Order(OrderDirection.ASC)
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/liquidity_pools?cursor=13537736921089&limit=200&order=asc",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that LiquidityPoolRequestBuilder.ForReserves correctly constructs URI with reserves parameter.
    /// </summary>
    [TestMethod]
    public void ForReserves_WithValidReserveAssets_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.LiquidityPools
            .ForReserves("EURT:GAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S",
                "PHP:GAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S")
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/liquidity_pools?reserves=EURT%3aGAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S%2cPHP%3aGAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that LiquidityPoolRequestBuilder.Execute correctly retrieves and deserializes liquidity pool page data.
    /// </summary>
    [TestMethod]
    public async Task Execute_WithDefaultParameters_ReturnsDeserializedLiquidityPoolPage()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/liquidityPoolPage.json");

        // Act
        var pages = await server.LiquidityPools.Execute();

        // Assert
        LiquidityPoolPageDeserializerTest.AssertTestData(pages);
    }

    /// <summary>
    ///     Verifies that LiquidityPoolRequestBuilder.Stream correctly streams and deserializes liquidity pool events.
    /// </summary>
    [TestMethod]
    public async Task Stream_WithValidJson_StreamsLiquidityPoolEvents()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("Responses/liquidityPool.json");
        var json = await File.ReadAllTextAsync(jsonPath);

        // Act & Assert
        var streamableTest =
            new StreamableTest<LiquidityPoolResponse>(json, LiquidityPoolDeserializerTest.AssertTestData);
        await streamableTest.Run();
    }
}