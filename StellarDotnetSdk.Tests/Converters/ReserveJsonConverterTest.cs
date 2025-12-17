using System;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Tests for ReserveJsonConverter.
///     Focus: serialization and deserialization of Reserve with native and credit assets.
/// </summary>
[TestClass]
public class ReserveJsonConverterTest
{
    private readonly JsonSerializerOptions _options = JsonOptions.DefaultOptions;

    /// <summary>
    ///     Tests round-trip serialization and deserialization of Reserve with native asset.
    ///     Verifies that native asset reserves are preserved through serialization cycle.
    /// </summary>
    [TestMethod]
    public void TestSerializeDeserializeNativeAsset()
    {
        var original = new Reserve
        {
            Asset = new AssetTypeNative(),
            Amount = "100.50",
        };

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Reserve>(json, _options);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("native", deserialized.Asset.CanonicalName());
        Assert.AreEqual("100.50", deserialized.Amount);
    }

    /// <summary>
    ///     Tests round-trip serialization and deserialization of Reserve with credit_alphanum4 asset.
    ///     Verifies that credit_alphanum4 asset reserves are preserved through serialization cycle.
    /// </summary>
    [TestMethod]
    public void TestSerializeDeserializeCreditAssetAlphaNum4()
    {
        var issuer = KeyPair.Random();
        var original = new Reserve
        {
            Asset = Asset.CreateNonNativeAsset("USD", issuer.AccountId),
            Amount = "500.0000000",
        };

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Reserve>(json, _options);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual($"USD:{issuer.AccountId}", deserialized.Asset.CanonicalName());
        Assert.AreEqual("500.0000000", deserialized.Amount);
    }

    /// <summary>
    ///     Tests round-trip serialization and deserialization of Reserve with credit_alphanum12 asset.
    ///     Verifies that credit_alphanum12 asset reserves are preserved through serialization cycle.
    /// </summary>
    [TestMethod]
    public void TestSerializeDeserializeCreditAssetAlphaNum12()
    {
        var issuer = KeyPair.Random();
        var original = new Reserve
        {
            Asset = Asset.CreateNonNativeAsset("LONGASSET123", issuer.AccountId),
            Amount = "1000.00",
        };

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Reserve>(json, _options);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual($"LONGASSET123:{issuer.AccountId}", deserialized.Asset.CanonicalName());
        Assert.AreEqual("1000.00", deserialized.Amount);
    }

    /// <summary>
    ///     Tests deserialization of valid JSON with native asset.
    ///     Verifies that JSON with native asset deserializes to Reserve with AssetTypeNative.
    /// </summary>
    [TestMethod]
    public void TestDeserializeValidJsonNative()
    {
        var json = @"{""asset"":""native"",""amount"":""250.50""}";

        var result = JsonSerializer.Deserialize<Reserve>(json, _options);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Asset, typeof(AssetTypeNative));
        Assert.AreEqual("250.50", result.Amount);
    }

    [TestMethod]
    public void TestDeserializeValidJsonCredit()
    {
        var issuer = KeyPair.Random();
        var json = $@"{{""asset"":""USD:{issuer.AccountId}"",""amount"":""100.00""}}";

        var result = JsonSerializer.Deserialize<Reserve>(json, _options);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Asset, typeof(AssetTypeCreditAlphaNum));
        Assert.AreEqual("100.00", result.Amount);
    }

    /// <summary>
    ///     Tests that deserialization throws ArgumentException when asset property is missing.
    ///     Verifies validation for required asset property.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeMissingAssetThrowsException()
    {
        var json = @"{""amount"":""100""}";
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws ArgumentException when amount property is missing.
    ///     Verifies validation for required amount property.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeMissingAmountThrowsException()
    {
        var json = @"{""asset"":""native""}";
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws ArgumentException when asset property is null.
    ///     Verifies validation for non-null asset property.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeNullAssetThrowsException()
    {
        var json = @"{""asset"":null,""amount"":""100""}";
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws ArgumentException when amount property is null.
    ///     Verifies validation for non-null amount property.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeNullAmountThrowsException()
    {
        var json = @"{""asset"":""native"",""amount"":null}";
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    /// <summary>
    ///     Tests deserialization handles large amount values correctly.
    ///     Verifies that maximum precision amounts are preserved.
    /// </summary>
    [TestMethod]
    public void TestDeserializeLargeAmount()
    {
        var json = @"{""asset"":""native"",""amount"":""922337203685477.5807""}";
        var result = JsonSerializer.Deserialize<Reserve>(json, _options);

        Assert.IsNotNull(result);
        Assert.AreEqual("922337203685477.5807", result.Amount);
    }

    /// <summary>
    ///     Tests deserialization handles zero amount values correctly.
    ///     Verifies that zero amounts are preserved correctly.
    /// </summary>
    [TestMethod]
    public void TestDeserializeZeroAmount()
    {
        var json = @"{""asset"":""native"",""amount"":""0""}";
        var result = JsonSerializer.Deserialize<Reserve>(json, _options);

        Assert.IsNotNull(result);
        Assert.AreEqual("0", result.Amount);
    }

    /// <summary>
    ///     Tests serialization produces correct JSON format with asset and amount properties.
    ///     Verifies that serialized JSON contains required properties with correct values.
    /// </summary>
    [TestMethod]
    public void TestSerializeFormat()
    {
        var issuer = KeyPair.Random();
        var reserve = new Reserve
        {
            Asset = Asset.CreateNonNativeAsset("XLM", issuer.AccountId),
            Amount = "100.50",
        };

        var json = JsonSerializer.Serialize(reserve, _options);

        Assert.IsTrue(json.Contains("\"asset\":"));
        Assert.IsTrue(json.Contains("\"amount\":"));
        Assert.IsTrue(json.Contains($"XLM:{issuer.AccountId}"));
        Assert.IsTrue(json.Contains("100.50"));
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException for malformed JSON.
    ///     Verifies proper error handling for invalid JSON syntax.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeInvalidJsonThrowsException()
    {
        // Missing closing brace
        var json = @"{""asset"":""native""";
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when JSON structure is wrong.
    ///     Verifies proper error handling for non-object JSON values.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeWrongStructureThrowsException()
    {
        // Not an object
        var json = @"""just a string""";
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws ArgumentException when asset property is empty string.
    ///     Verifies validation for non-empty asset property.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeEmptyAssetThrowsException()
    {
        var json = @"{""asset"":"""",""amount"":""100""}";
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws ArgumentException when amount property is empty string.
    ///     Verifies validation for non-empty amount property.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeEmptyAmountThrowsException()
    {
        var json = @"{""asset"":""native"",""amount"":""""}";
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization ignores extra properties in JSON.
    ///     Verifies that unknown properties do not cause errors and are ignored.
    /// </summary>
    [TestMethod]
    public void TestDeserializeExtraPropertiesIgnored()
    {
        var json = @"{""asset"":""native"",""amount"":""100"",""extra_property"":""ignored""}";
        var result = JsonSerializer.Deserialize<Reserve>(json, _options);

        Assert.IsNotNull(result);
        Assert.AreEqual("native", result.Asset.CanonicalName());
        Assert.AreEqual("100", result.Amount);
    }
}