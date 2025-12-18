using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
///     Unit tests for <see cref="PathStrictReceiveRequestBuilder" /> class.
/// </summary>
[TestClass]
public class PathStrictReceiveRequestBuilderTest
{
    /// <summary>
    ///     Verifies that PathStrictReceiveRequestBuilder.BuildUri correctly constructs URI with source account, source assets,
    ///     destination account, destination asset, and destination amount.
    /// </summary>
    /// <remarks>
    ///     Technically not a valid request since it contains both a source account and assets.
    /// </remarks>
    [TestMethod]
    public void BuildUri_WithAllParameters_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");
        var destinationAsset =
            Asset.CreateNonNativeAsset("USD", "GAEDTJ4PPEFVW5XV2S7LUXBEHNQMX5Q2GM562RJGOQG7GVCE5H3HIB4V");

        // Act
        var req = server.PathStrictReceive
            .SourceAccount("GARSFJNXJIHO6ULUBK3DBYKVSIZE7SC72S5DYBCHU7DKL22UXKVD7MXP")
            .SourceAssets(new Asset[] { new AssetTypeNative(), destinationAsset })
            .DestinationAccount("GAEDTJ4PPEFVW5XV2S7LUXBEHNQMX5Q2GM562RJGOQG7GVCE5H3HIB4V")
            .DestinationAsset(destinationAsset)
            .DestinationAmount("10.1");

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/paths/strict-receive?" +
            "source_account=GARSFJNXJIHO6ULUBK3DBYKVSIZE7SC72S5DYBCHU7DKL22UXKVD7MXP&" +
            "source_assets=native%2cUSD%3aGAEDTJ4PPEFVW5XV2S7LUXBEHNQMX5Q2GM562RJGOQG7GVCE5H3HIB4V&" +
            "destination_account=GAEDTJ4PPEFVW5XV2S7LUXBEHNQMX5Q2GM562RJGOQG7GVCE5H3HIB4V&" +
            "destination_asset_type=credit_alphanum4&" +
            "destination_asset_code=USD&" +
            "destination_asset_issuer=GAEDTJ4PPEFVW5XV2S7LUXBEHNQMX5Q2GM562RJGOQG7GVCE5H3HIB4V&" +
            "destination_amount=10.1",
            req.BuildUri().ToString());
    }

    /// <summary>
    ///     Verifies that PathStrictReceiveRequestBuilder.Execute correctly retrieves and deserializes path page data.
    /// </summary>
    [TestMethod]
    public async Task Execute_WithSourceAccountDestinationAccountAndAsset_ReturnsDeserializedPathPage()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/pathPage.json");
        var destinationAsset =
            Asset.CreateNonNativeAsset("USD", "GAEDTJ4PPEFVW5XV2S7LUXBEHNQMX5Q2GM562RJGOQG7GVCE5H3HIB4V");

        // Act
        var assets = await server.PathStrictReceive
            .SourceAccount("GARSFJNXJIHO6ULUBK3DBYKVSIZE7SC72S5DYBCHU7DKL22UXKVD7MXP")
            .DestinationAccount("GAEDTJ4PPEFVW5XV2S7LUXBEHNQMX5Q2GM562RJGOQG7GVCE5H3HIB4V")
            .DestinationAsset(destinationAsset)
            .DestinationAmount("10.1")
            .Execute();

        // Assert
        PathsPageDeserializerTest.AssertTestData(assets);
    }
}