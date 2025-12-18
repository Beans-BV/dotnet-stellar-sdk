using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
///     Unit tests for <see cref="PathStrictSendRequestBuilder" /> class.
/// </summary>
[TestClass]
public class PathStrictSendRequestBuilderTest
{
    /// <summary>
    ///     Verifies that PathStrictSendRequestBuilder.BuildUri correctly constructs URI with source amount, source asset,
    ///     destination account, and destination assets.
    /// </summary>
    /// <remarks>
    ///     Technically not a valid request since it contains both a destination account and assets.
    /// </remarks>
    [TestMethod]
    public void BuildUri_WithAllParameters_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");
        var sourceAsset =
            Asset.CreateNonNativeAsset("USD", "GAEDTJ4PPEFVW5XV2S7LUXBEHNQMX5Q2GM562RJGOQG7GVCE5H3HIB4V");

        // Act
        var req = server.PathStrictSend
            .SourceAmount("10.1")
            .SourceAsset(sourceAsset)
            .DestinationAccount("GAEDTJ4PPEFVW5XV2S7LUXBEHNQMX5Q2GM562RJGOQG7GVCE5H3HIB4V")
            .DestinationAssets(new Asset[] { new AssetTypeNative(), sourceAsset });

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/paths/strict-send?" +
            "source_amount=10.1&" +
            "source_asset_type=credit_alphanum4&" +
            "source_asset_code=USD&" +
            "source_asset_issuer=GAEDTJ4PPEFVW5XV2S7LUXBEHNQMX5Q2GM562RJGOQG7GVCE5H3HIB4V&" +
            "destination_account=GAEDTJ4PPEFVW5XV2S7LUXBEHNQMX5Q2GM562RJGOQG7GVCE5H3HIB4V&" +
            "destination_assets=native%2cUSD%3aGAEDTJ4PPEFVW5XV2S7LUXBEHNQMX5Q2GM562RJGOQG7GVCE5H3HIB4V",
            req.BuildUri().ToString());
    }

    /// <summary>
    ///     Verifies that PathStrictSendRequestBuilder.Execute correctly retrieves and deserializes path page data.
    /// </summary>
    [TestMethod]
    public async Task Execute_WithSourceAmountAssetAndDestinationAssets_ReturnsDeserializedPathPage()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/pathPage.json");
        var sourceAsset =
            Asset.CreateNonNativeAsset("USD", "GAEDTJ4PPEFVW5XV2S7LUXBEHNQMX5Q2GM562RJGOQG7GVCE5H3HIB4V");

        // Act
        var assets = await server.PathStrictSend
            .SourceAmount("10.1")
            .SourceAsset(sourceAsset)
            .DestinationAssets(new Asset[] { new AssetTypeNative(), sourceAsset })
            .Execute();

        // Assert
        PathsPageDeserializerTest.AssertTestData(assets);
    }
}