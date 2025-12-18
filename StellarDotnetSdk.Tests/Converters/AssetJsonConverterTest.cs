using System;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Tests for AssetJsonConverter.
///     Focus: serialization and deserialization of Asset types (native, credit_alphanum4, credit_alphanum12).
/// </summary>
[TestClass]
public class AssetJsonConverterTest
{
    private readonly JsonSerializerOptions _options = new()
    {
        Converters = { new AssetJsonConverter() },
    };

    /// <summary>
    ///     Tests serialization of native asset type.
    ///     Verifies that native assets serialize to JSON with asset_type property set to "native".
    /// </summary>
    [TestMethod]
    public void Serialize_WithNativeAsset_ProducesCorrectJson()
    {
        // Arrange
        var asset = new AssetTypeNative();

        // Act
        var json = JsonSerializer.Serialize<Asset>(asset, _options);

        // Assert
        Assert.IsTrue(json.Contains("\"asset_type\":\"native\""));
    }

    /// <summary>
    ///     Tests serialization of credit_alphanum4 asset type.
    ///     Verifies that credit_alphanum4 assets serialize with asset_type, asset_code, and asset_issuer properties.
    /// </summary>
    [TestMethod]
    public void Serialize_WithCreditAlphaNum4_ProducesCorrectJson()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var asset = new AssetTypeCreditAlphaNum4("USD", issuer.AccountId);

        // Act
        var json = JsonSerializer.Serialize<Asset>(asset, _options);

        // Assert
        Assert.IsTrue(json.Contains("\"asset_type\":\"credit_alphanum4\""));
        Assert.IsTrue(json.Contains("\"asset_code\":\"USD\""));
        Assert.IsTrue(json.Contains($"\"asset_issuer\":\"{issuer.AccountId}\""));
    }

    /// <summary>
    ///     Tests serialization of credit_alphanum12 asset type.
    ///     Verifies that credit_alphanum12 assets serialize with asset_type, asset_code, and asset_issuer properties.
    /// </summary>
    [TestMethod]
    public void Serialize_WithCreditAlphaNum12_ProducesCorrectJson()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var asset = new AssetTypeCreditAlphaNum12("TESTTEST", issuer.AccountId);

        // Act
        var json = JsonSerializer.Serialize<Asset>(asset, _options);

        // Assert
        Assert.IsTrue(json.Contains("\"asset_type\":\"credit_alphanum12\""));
        Assert.IsTrue(json.Contains("\"asset_code\":\"TESTTEST\""));
        Assert.IsTrue(json.Contains($"\"asset_issuer\":\"{issuer.AccountId}\""));
    }

    /// <summary>
    ///     Tests deserialization of native asset from JSON.
    ///     Verifies that JSON with asset_type "native" deserializes to AssetTypeNative instance.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithNativeAssetJson_ReturnsNativeAsset()
    {
        // Arrange
        var json = @"{""asset_type"":""native""}";

        // Act
        var asset = JsonSerializer.Deserialize<Asset>(json, _options);

        // Assert
        Assert.IsNotNull(asset);
        Assert.IsInstanceOfType(asset, typeof(AssetTypeNative));
    }

    /// <summary>
    ///     Tests deserialization of credit_alphanum4 asset from JSON.
    ///     Verifies that JSON with credit_alphanum4 asset_type deserializes to AssetTypeCreditAlphaNum4 with correct code and
    ///     issuer.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithCreditAlphaNum4Json_ReturnsCreditAlphaNum4Asset()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var json =
            $@"{{""asset_type"":""credit_alphanum4"",""asset_code"":""USD"",""asset_issuer"":""{issuer.AccountId}""}}";

        // Act
        var asset = JsonSerializer.Deserialize<Asset>(json, _options);

        // Assert
        Assert.IsNotNull(asset);
        Assert.IsInstanceOfType(asset, typeof(AssetTypeCreditAlphaNum4));
        var creditAsset = (AssetTypeCreditAlphaNum4)asset;
        Assert.AreEqual("USD", creditAsset.Code);
        Assert.AreEqual(issuer.AccountId, creditAsset.Issuer);
    }

    /// <summary>
    ///     Tests deserialization of credit_alphanum12 asset from JSON.
    ///     Verifies that JSON with credit_alphanum12 asset_type deserializes to AssetTypeCreditAlphaNum12 with correct code
    ///     and issuer.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithCreditAlphaNum12Json_ReturnsCreditAlphaNum12Asset()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var json =
            $@"{{""asset_type"":""credit_alphanum12"",""asset_code"":""TESTTEST"",""asset_issuer"":""{issuer.AccountId}""}}";

        // Act
        var asset = JsonSerializer.Deserialize<Asset>(json, _options);

        // Assert
        Assert.IsNotNull(asset);
        Assert.IsInstanceOfType(asset, typeof(AssetTypeCreditAlphaNum12));
        var creditAsset = (AssetTypeCreditAlphaNum12)asset;
        Assert.AreEqual("TESTTEST", creditAsset.Code);
        Assert.AreEqual(issuer.AccountId, creditAsset.Issuer);
    }

    /// <summary>
    ///     Tests that deserialization throws ArgumentException when asset_type property is missing.
    ///     Verifies proper error handling for invalid JSON structure.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Deserialize_WithMissingAssetType_ThrowsArgumentException()
    {
        // Arrange
        var json =
            @"{""asset_code"":""USD"",""asset_issuer"":""GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR""}";

        // Act & Assert
        JsonSerializer.Deserialize<Asset>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws ArgumentException when credit asset is missing asset_code property.
    ///     Verifies validation for required properties on non-native assets.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Deserialize_WithNonNativeMissingCode_ThrowsArgumentException()
    {
        // Arrange
        var json =
            @"{""asset_type"":""credit_alphanum4"",""asset_issuer"":""GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR""}";

        // Act & Assert
        JsonSerializer.Deserialize<Asset>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws ArgumentException when credit asset is missing asset_issuer property.
    ///     Verifies validation for required properties on non-native assets.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Deserialize_WithNonNativeMissingIssuer_ThrowsArgumentException()
    {
        // Arrange
        var json = @"{""asset_type"":""credit_alphanum4"",""asset_code"":""USD""}";

        // Act & Assert
        JsonSerializer.Deserialize<Asset>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when JSON is not an object.
    ///     Verifies proper error handling for invalid JSON token types.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithNonObjectJson_ThrowsJsonException()
    {
        // Arrange
        var json = @"""just a string""";

        // Act & Assert
        JsonSerializer.Deserialize<Asset>(json, _options);
    }

    /// <summary>
    ///     Tests round-trip serialization and deserialization of native asset.
    ///     Verifies that serialized native asset can be deserialized back to the same type.
    /// </summary>
    [TestMethod]
    public void RoundTrip_WithNativeAsset_RoundTripsCorrectly()
    {
        // Arrange
        var original = new AssetTypeNative();

        // Act
        var json = JsonSerializer.Serialize<Asset>(original, _options);
        var deserialized = JsonSerializer.Deserialize<Asset>(json, _options);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.IsInstanceOfType(deserialized, typeof(AssetTypeNative));
    }

    /// <summary>
    ///     Tests round-trip serialization and deserialization of credit_alphanum4 asset.
    ///     Verifies that serialized credit_alphanum4 asset can be deserialized back with preserved code and issuer.
    /// </summary>
    [TestMethod]
    public void RoundTrip_WithCreditAlphaNum4_RoundTripsCorrectly()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var original = new AssetTypeCreditAlphaNum4("USD", issuer.AccountId);

        // Act
        var json = JsonSerializer.Serialize<Asset>(original, _options);
        var deserialized = JsonSerializer.Deserialize<Asset>(json, _options);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.IsInstanceOfType(deserialized, typeof(AssetTypeCreditAlphaNum4));
        var creditAsset = (AssetTypeCreditAlphaNum4)deserialized;
        Assert.AreEqual("USD", creditAsset.Code);
        Assert.AreEqual(issuer.AccountId, creditAsset.Issuer);
    }

    /// <summary>
    ///     Tests round-trip serialization and deserialization of credit_alphanum12 asset.
    ///     Verifies that serialized credit_alphanum12 asset can be deserialized back with preserved code and issuer.
    /// </summary>
    [TestMethod]
    public void RoundTrip_WithCreditAlphaNum12_RoundTripsCorrectly()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var original = new AssetTypeCreditAlphaNum12("TESTTEST", issuer.AccountId);

        // Act
        var json = JsonSerializer.Serialize<Asset>(original, _options);
        var deserialized = JsonSerializer.Deserialize<Asset>(json, _options);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.IsInstanceOfType(deserialized, typeof(AssetTypeCreditAlphaNum12));
        var creditAsset = (AssetTypeCreditAlphaNum12)deserialized;
        Assert.AreEqual("TESTTEST", creditAsset.Code);
        Assert.AreEqual(issuer.AccountId, creditAsset.Issuer);
    }
}