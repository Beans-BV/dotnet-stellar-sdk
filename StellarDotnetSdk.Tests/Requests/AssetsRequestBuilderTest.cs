using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
/// Unit tests for <see cref="AssetsRequestBuilder"/> class.
/// </summary>
[TestClass]
public class AssetsRequestBuilderTest
{
    /// <summary>
    /// Verifies that AssetsRequestBuilder.BuildUri correctly constructs URI with limit and order parameters.
    /// </summary>
    [TestMethod]
    public void BuildUri_WithLimitAndOrder_BuildsCorrectUri()
    {
        // Arrange
        var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Assets
            .Limit(200)
            .Order(OrderDirection.DESC)
            .BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/assets?limit=200&order=desc", uri.ToString());
    }

    /// <summary>
    /// Verifies that AssetsRequestBuilder.AssetCode correctly adds asset code parameter to URI.
    /// </summary>
    [TestMethod]
    public void AssetCode_WithValidAssetCode_AddsAssetCodeParameter()
    {
        // Arrange
        var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Assets
            .AssetCode("USD")
            .BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/assets?asset_code=USD", uri.ToString());
    }

    /// <summary>
    /// Verifies that AssetsRequestBuilder.AssetIssuer correctly adds asset issuer parameter to URI.
    /// </summary>
    [TestMethod]
    public void AssetIssuer_WithValidIssuerId_AddsAssetIssuerParameter()
    {
        // Arrange
        var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Assets
            .AssetIssuer("GA2HGBJIJKI6O4XEM7CZWY5PS6GKSXL6D34ERAJYQSPYA6X6AI7HYW36")
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/assets?asset_issuer=GA2HGBJIJKI6O4XEM7CZWY5PS6GKSXL6D34ERAJYQSPYA6X6AI7HYW36",
            uri.ToString());
    }

    /// <summary>
    /// Verifies that AssetsRequestBuilder.Execute correctly retrieves and deserializes asset page data.
    /// </summary>
    [TestMethod]
    public async Task Execute_WithAssetCode_ReturnsDeserializedAssetPage()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/assetPage.json");
        // the asset code string really doesn't matter for testing, as the response is static for testing purposes...

        // Act
        var assetsPage = await server.Assets.AssetCode("")
            .Execute();

        // Assert
        AssetPageDeserializerTest.AssertTestData(assetsPage);
    }
}