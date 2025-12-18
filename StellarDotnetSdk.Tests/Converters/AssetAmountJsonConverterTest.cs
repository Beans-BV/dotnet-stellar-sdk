using System;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
/// Tests for AssetAmountJsonConverter JSON serialization and deserialization functionality.
/// </summary>
[TestClass]
public class AssetAmountJsonConverterTest
{
    private readonly JsonSerializerOptions _options = JsonOptions.DefaultOptions;

    /// <summary>
    /// Verifies that native asset amounts serialize and deserialize correctly through JSON.
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithNativeAsset_RoundTripsCorrectly()
    {
        // Arrange
        var original = new AssetAmount(new AssetTypeNative(), "100.50");

        // Act
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<AssetAmount>(json, _options);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual("native", deserialized.Asset.CanonicalName());
        Assert.AreEqual("100.50", deserialized.Amount);
    }

    /// <summary>
    /// Verifies that CreditAlphaNum4 asset amounts serialize and deserialize correctly through JSON.
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithCreditAlphaNum4Asset_RoundTripsCorrectly()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var original = new AssetAmount(
            Asset.CreateNonNativeAsset("USD", issuer.AccountId),
            "500.0000000"
        );

        // Act
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<AssetAmount>(json, _options);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual($"USD:{issuer.AccountId}", deserialized.Asset.CanonicalName());
        Assert.AreEqual("500.0000000", deserialized.Amount);
    }

    /// <summary>
    /// Verifies that CreditAlphaNum12 asset amounts serialize and deserialize correctly through JSON.
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithCreditAlphaNum12Asset_RoundTripsCorrectly()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var original = new AssetAmount(
            Asset.CreateNonNativeAsset("LONGASSET123", issuer.AccountId),
            "1000.00"
        );

        // Act
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<AssetAmount>(json, _options);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual($"LONGASSET123:{issuer.AccountId}", deserialized.Asset.CanonicalName());
        Assert.AreEqual("1000.00", deserialized.Amount);
    }

    /// <summary>
    /// Verifies that deserializing valid JSON with native asset creates correct AssetAmount instance.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithValidJsonNativeAsset_CreatesCorrectInstance()
    {
        // Arrange
        var json = @"{""asset"":""native"",""amount"":""250.50""}";

        // Act
        var result = JsonSerializer.Deserialize<AssetAmount>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Asset, typeof(AssetTypeNative));
        Assert.AreEqual("250.50", result.Amount);
    }

    /// <summary>
    /// Verifies that deserializing valid JSON with non-native asset creates correct AssetAmount instance.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithValidJsonNonNativeAsset_CreatesCorrectInstance()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var json = $@"{{""asset"":""USD:{issuer.AccountId}"",""amount"":""100.00""}}";

        // Act
        var result = JsonSerializer.Deserialize<AssetAmount>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Asset, typeof(AssetTypeCreditAlphaNum));
        Assert.AreEqual("100.00", result.Amount);
    }

    /// <summary>
    /// Verifies that deserializing JSON with missing asset property throws ArgumentException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Deserialize_WithMissingAsset_ThrowsArgumentException()
    {
        // Arrange
        var json = @"{""amount"":""100""}";

        // Act
        JsonSerializer.Deserialize<AssetAmount>(json, _options);
    }

    /// <summary>
    /// Verifies that deserializing JSON with missing amount property throws ArgumentException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Deserialize_WithMissingAmount_ThrowsArgumentException()
    {
        // Arrange
        var json = @"{""asset"":""native""}";

        // Act
        JsonSerializer.Deserialize<AssetAmount>(json, _options);
    }

    /// <summary>
    /// Verifies that deserializing JSON with null asset property throws ArgumentException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Deserialize_WithNullAsset_ThrowsArgumentException()
    {
        // Arrange
        var json = @"{""asset"":null,""amount"":""100""}";

        // Act
        JsonSerializer.Deserialize<AssetAmount>(json, _options);
    }

    /// <summary>
    /// Verifies that deserializing JSON with null amount property throws ArgumentException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Deserialize_WithNullAmount_ThrowsArgumentException()
    {
        // Arrange
        var json = @"{""asset"":""native"",""amount"":null}";

        // Act
        JsonSerializer.Deserialize<AssetAmount>(json, _options);
    }

    /// <summary>
    /// Verifies that deserializing JSON with large amount value succeeds.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithLargeAmount_Succeeds()
    {
        // Arrange
        var json = @"{""asset"":""native"",""amount"":""922337203685477.5807""}";

        // Act
        var result = JsonSerializer.Deserialize<AssetAmount>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("922337203685477.5807", result.Amount);
    }

    /// <summary>
    /// Verifies that deserializing JSON with zero amount value succeeds.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithZeroAmount_Succeeds()
    {
        // Arrange
        var json = @"{""asset"":""native"",""amount"":""0""}";

        // Act
        var result = JsonSerializer.Deserialize<AssetAmount>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("0", result.Amount);
    }

    /// <summary>
    /// Verifies that serialization produces JSON with correct property names and values.
    /// </summary>
    [TestMethod]
    public void Serialize_ProducesCorrectJsonFormat()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var assetAmount = new AssetAmount(
            Asset.CreateNonNativeAsset("XLM", issuer.AccountId),
            "100.50"
        );

        // Act
        var json = JsonSerializer.Serialize(assetAmount, _options);

        // Assert
        Assert.IsTrue(json.Contains("\"asset\":"));
        Assert.IsTrue(json.Contains("\"amount\":"));
        Assert.IsTrue(json.Contains($"XLM:{issuer.AccountId}"));
        Assert.IsTrue(json.Contains("100.50"));
    }

    /// <summary>
    /// Verifies that deserializing invalid JSON throws JsonException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange - Missing closing brace
        var json = @"{""asset"":""native""";

        // Act
        JsonSerializer.Deserialize<AssetAmount>(json, _options);
    }

    /// <summary>
    /// Verifies that deserializing JSON with wrong structure throws JsonException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithWrongStructure_ThrowsJsonException()
    {
        // Arrange - Not an object
        var json = @"""just a string""";

        // Act
        JsonSerializer.Deserialize<AssetAmount>(json, _options);
    }

    /// <summary>
    /// Verifies that deserializing JSON with empty asset property throws ArgumentException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Deserialize_WithEmptyAsset_ThrowsArgumentException()
    {
        // Arrange
        var json = @"{""asset"":"""",""amount"":""100""}";

        // Act
        JsonSerializer.Deserialize<AssetAmount>(json, _options);
    }
}