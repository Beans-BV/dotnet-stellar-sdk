using System;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
/// Tests for ReserveJsonConverter JSON serialization and deserialization functionality.
/// </summary>
[TestClass]
public class ReserveJsonConverterTest
{
    private readonly JsonSerializerOptions _options = JsonOptions.DefaultOptions;

    /// <summary>
    /// Verifies that native asset reserves serialize and deserialize correctly through JSON.
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithNativeAsset_RoundTripsCorrectly()
    {
        // Arrange
        var original = new Reserve
        {
            Asset = new AssetTypeNative(),
            Amount = "100.50",
        };

        // Act
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Reserve>(json, _options);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual("native", deserialized.Asset.CanonicalName());
        Assert.AreEqual("100.50", deserialized.Amount);
    }

    /// <summary>
    /// Verifies that CreditAlphaNum4 asset reserves serialize and deserialize correctly through JSON.
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithCreditAlphaNum4Asset_RoundTripsCorrectly()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var original = new Reserve
        {
            Asset = Asset.CreateNonNativeAsset("USD", issuer.AccountId),
            Amount = "500.0000000",
        };

        // Act
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Reserve>(json, _options);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual($"USD:{issuer.AccountId}", deserialized.Asset.CanonicalName());
        Assert.AreEqual("500.0000000", deserialized.Amount);
    }

    /// <summary>
    /// Verifies that CreditAlphaNum12 asset reserves serialize and deserialize correctly through JSON.
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithCreditAlphaNum12Asset_RoundTripsCorrectly()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var original = new Reserve
        {
            Asset = Asset.CreateNonNativeAsset("LONGASSET123", issuer.AccountId),
            Amount = "1000.00",
        };

        // Act
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Reserve>(json, _options);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual($"LONGASSET123:{issuer.AccountId}", deserialized.Asset.CanonicalName());
        Assert.AreEqual("1000.00", deserialized.Amount);
    }

    /// <summary>
    /// Verifies that deserializing valid JSON with native asset creates correct Reserve instance.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithValidJsonNativeAsset_CreatesCorrectInstance()
    {
        // Arrange
        var json = @"{""asset"":""native"",""amount"":""250.50""}";

        // Act
        var result = JsonSerializer.Deserialize<Reserve>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Asset, typeof(AssetTypeNative));
        Assert.AreEqual("250.50", result.Amount);
    }

    /// <summary>
    /// Verifies that deserializing valid JSON with credit asset creates correct Reserve instance.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithValidJsonCreditAsset_CreatesCorrectInstance()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var json = $@"{{""asset"":""USD:{issuer.AccountId}"",""amount"":""100.00""}}";

        // Act
        var result = JsonSerializer.Deserialize<Reserve>(json, _options);

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
        JsonSerializer.Deserialize<Reserve>(json, _options);
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
        JsonSerializer.Deserialize<Reserve>(json, _options);
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
        JsonSerializer.Deserialize<Reserve>(json, _options);
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
        JsonSerializer.Deserialize<Reserve>(json, _options);
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
        var result = JsonSerializer.Deserialize<Reserve>(json, _options);

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
        var result = JsonSerializer.Deserialize<Reserve>(json, _options);

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
        var reserve = new Reserve
        {
            Asset = Asset.CreateNonNativeAsset("XLM", issuer.AccountId),
            Amount = "100.50",
        };

        // Act
        var json = JsonSerializer.Serialize(reserve, _options);

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
        JsonSerializer.Deserialize<Reserve>(json, _options);
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
        JsonSerializer.Deserialize<Reserve>(json, _options);
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
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    /// <summary>
    /// Verifies that deserializing JSON with empty amount property throws ArgumentException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Deserialize_WithEmptyAmount_ThrowsArgumentException()
    {
        // Arrange
        var json = @"{""asset"":""native"",""amount"":""""}";

        // Act
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    /// <summary>
    /// Verifies that deserializing JSON with extra properties ignores them and creates correct instance.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithExtraProperties_IgnoresExtraProperties()
    {
        // Arrange
        var json = @"{""asset"":""native"",""amount"":""100"",""extra_property"":""ignored""}";

        // Act
        var result = JsonSerializer.Deserialize<Reserve>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("native", result.Asset.CanonicalName());
        Assert.AreEqual("100", result.Amount);
    }
}