using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
///     Unit tests for <see cref="PathsRequestBuilder" /> class.
/// </summary>
[TestClass]
public class PathsRequestBuilderTest
{
    /// <summary>
    ///     Verifies that PathsRequestBuilder.BuildUri correctly constructs URI with default parameters.
    /// </summary>
    [TestMethod]
    public void BuildUri_WithDefaultParameters_BuildsCorrectUri()
    {
        // Arrange
        using var httpClient = Utils.CreateFakeHttpClient("");
        var pathsBuilder = new PathsRequestBuilder(new Uri("https://horizon-testnet.stellar.org"), httpClient);

        // Act
        var uri = pathsBuilder.BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/paths", uri.ToString());
    }

    /// <summary>
    ///     Verifies that PathsRequestBuilder.DestinationAccount correctly adds destination_account parameter to URI.
    /// </summary>
    [TestMethod]
    public void DestinationAccount_WithValidAccountId_AddsDestinationAccountParameter()
    {
        // Arrange
        using var httpClient = Utils.CreateFakeHttpClient("");
        var pathsBuilder = new PathsRequestBuilder(new Uri("https://horizon-testnet.stellar.org"), httpClient);

        // Act
        var uri = pathsBuilder
            .DestinationAccount("GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H")
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/paths?destination_account=GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that PathsRequestBuilder.SourceAccount correctly adds source_account parameter to URI.
    /// </summary>
    [TestMethod]
    public void SourceAccount_WithValidAccountId_AddsSourceAccountParameter()
    {
        // Arrange
        using var httpClient = Utils.CreateFakeHttpClient("");
        var pathsBuilder = new PathsRequestBuilder(new Uri("https://horizon-testnet.stellar.org"), httpClient);

        // Act
        var uri = pathsBuilder
            .SourceAccount("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7")
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/paths?source_account=GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that PathsRequestBuilder.DestinationAmount correctly adds destination_amount parameter to URI.
    /// </summary>
    [TestMethod]
    public void DestinationAmount_WithValidAmount_AddsDestinationAmountParameter()
    {
        // Arrange
        using var httpClient = Utils.CreateFakeHttpClient("");
        var pathsBuilder = new PathsRequestBuilder(new Uri("https://horizon-testnet.stellar.org"), httpClient);

        // Act
        var uri = pathsBuilder
            .DestinationAmount("20.0000000")
            .BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/paths?destination_amount=20.0000000", uri.ToString());
    }

    /// <summary>
    ///     Verifies that PathsRequestBuilder.DestinationAsset correctly adds destination asset parameters for credit alphanum asset.
    /// </summary>
    [TestMethod]
    public void DestinationAsset_WithCreditAlphaNumAsset_AddsDestinationAssetParameters()
    {
        // Arrange
        using var httpClient = Utils.CreateFakeHttpClient("");
        var pathsBuilder = new PathsRequestBuilder(new Uri("https://horizon-testnet.stellar.org"), httpClient);
        var asset = Asset.CreateNonNativeAsset("EUR", "GDSBCQO34HWPGUGQSP3QBFEXVTSR2PW46UIGTHVWGWJGQKH3AFNHXHXN");

        // Act
        var uri = pathsBuilder
            .DestinationAsset(asset)
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/paths?destination_asset_type=credit_alphanum4&destination_asset_code=EUR&destination_asset_issuer=GDSBCQO34HWPGUGQSP3QBFEXVTSR2PW46UIGTHVWGWJGQKH3AFNHXHXN",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that PathsRequestBuilder.DestinationAsset correctly adds destination asset parameters for native asset.
    /// </summary>
    [TestMethod]
    public void DestinationAsset_WithNativeAsset_AddsOnlyDestinationAssetType()
    {
        // Arrange
        using var httpClient = Utils.CreateFakeHttpClient("");
        var pathsBuilder = new PathsRequestBuilder(new Uri("https://horizon-testnet.stellar.org"), httpClient);
        var asset = Asset.Create("native");

        // Act
        var uri = pathsBuilder
            .DestinationAsset(asset)
            .BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/paths?destination_asset_type=native", uri.ToString());
    }

    /// <summary>
    ///     Verifies that PathsRequestBuilder correctly combines all parameters to build URI.
    /// </summary>
    [TestMethod]
    public void BuildUri_WithAllParameters_BuildsCorrectUri()
    {
        // Arrange
        using var httpClient = Utils.CreateFakeHttpClient("");
        var pathsBuilder = new PathsRequestBuilder(new Uri("https://horizon-testnet.stellar.org"), httpClient);
        var asset = Asset.CreateNonNativeAsset("USD", "GDSBCQO34HWPGUGQSP3QBFEXVTSR2PW46UIGTHVWGWJGQKH3AFNHXHXN");

        // Act
        var uri = pathsBuilder
            .SourceAccount("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7")
            .DestinationAccount("GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H")
            .DestinationAmount("20.0000000")
            .DestinationAsset(asset)
            .Limit(10)
            .Order(OrderDirection.ASC)
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/paths?source_account=GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7&destination_account=GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H&destination_amount=20.0000000&destination_asset_type=credit_alphanum4&destination_asset_code=USD&destination_asset_issuer=GDSBCQO34HWPGUGQSP3QBFEXVTSR2PW46UIGTHVWGWJGQKH3AFNHXHXN&limit=10&order=asc",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that PathsRequestBuilder.Execute correctly retrieves and deserializes paths page data.
    /// </summary>
    [TestMethod]
    public async Task Execute_WithAllParameters_ReturnsDeserializedPathsPage()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("Responses/pathPage.json");
        var json = await File.ReadAllTextAsync(jsonPath);
        using var httpClient = Utils.CreateFakeHttpClient(json);
        var pathsBuilder = new PathsRequestBuilder(new Uri("https://horizon-testnet.stellar.org"), httpClient);
        var asset = Asset.CreateNonNativeAsset("EUR", "GDSBCQO34HWPGUGQSP3QBFEXVTSR2PW46UIGTHVWGWJGQKH3AFNHXHXN");

        // Act
        var pathsPage = await pathsBuilder
            .SourceAccount("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7")
            .DestinationAccount("GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H")
            .DestinationAmount("20.0000000")
            .DestinationAsset(asset)
            .Execute();

        // Assert
        PathsPageDeserializerTest.AssertTestData(pathsPage);
    }
}

