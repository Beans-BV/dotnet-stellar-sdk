using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Tests.Responses;

/// <summary>
///     Unit tests for deserializing asset responses from JSON.
/// </summary>
[TestClass]
public class AssetDeserializerTest
{
    /// <summary>
    ///     Verifies that native Asset can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithNativeAssetJson_ReturnsNativeAsset()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("assetAssetTypeNative.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var asset = JsonSerializer.Deserialize<Asset>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(asset);
        Assert.AreEqual(asset.Type, "native");
    }

    /// <summary>
    ///     Verifies that credit Asset can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithCreditAssetJson_ReturnsCreditAsset()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("assetAssetTypeCredit.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var asset = JsonSerializer.Deserialize<Asset>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(asset);
        Assert.AreEqual(asset.Type, "credit_alphanum4");
        var creditAsset = (AssetTypeCreditAlphaNum)asset;
        Assert.AreEqual(creditAsset.Code, "CNY");
        Assert.AreEqual(creditAsset.Issuer, "GAREELUB43IRHWEASCFBLKHURCGMHE5IF6XSE7EXDLACYHGRHM43RFOX");
    }
}