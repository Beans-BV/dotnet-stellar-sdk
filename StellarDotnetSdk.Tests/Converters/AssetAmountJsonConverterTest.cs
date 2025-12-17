using System;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Tests for AssetAmountJsonConverter.
///     Focus: serialization and deserialization of AssetAmount with native and credit assets.
/// </summary>
[TestClass]
public class AssetAmountJsonConverterTest
{
    private readonly JsonSerializerOptions _options = JsonOptions.DefaultOptions;

    /// <summary>
    ///     Verifies round-trip serialization/deserialization of AssetAmount with native asset.
    /// </summary>
    [TestMethod]
    public void TestSerializeDeserializeNativeAsset()
    {
        var original = new AssetAmount(new AssetTypeNative(), "100.50");

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<AssetAmount>(json, _options);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("native", deserialized.Asset.CanonicalName());
        Assert.AreEqual("100.50", deserialized.Amount);
    }

    /// <summary>
    ///     Verifies round-trip serialization/deserialization of AssetAmount with credit_alphanum4 asset.
    /// </summary>
    [TestMethod]
    public void TestSerializeDeserializeCreditAssetAlphaNum4()
    {
        var issuer = KeyPair.Random();
        var original = new AssetAmount(
            Asset.CreateNonNativeAsset("USD", issuer.AccountId),
            "500.0000000"
        );

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<AssetAmount>(json, _options);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual($"USD:{issuer.AccountId}", deserialized.Asset.CanonicalName());
        Assert.AreEqual("500.0000000", deserialized.Amount);
    }

    /// <summary>
    ///     Verifies round-trip serialization/deserialization of AssetAmount with credit_alphanum12 asset.
    /// </summary>
    [TestMethod]
    public void TestSerializeDeserializeCreditAssetAlphaNum12()
    {
        var issuer = KeyPair.Random();
        var original = new AssetAmount(
            Asset.CreateNonNativeAsset("LONGASSET123", issuer.AccountId),
            "1000.00"
        );

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<AssetAmount>(json, _options);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual($"LONGASSET123:{issuer.AccountId}", deserialized.Asset.CanonicalName());
        Assert.AreEqual("1000.00", deserialized.Amount);
    }

    /// <summary>
    ///     Verifies deserialization of valid JSON with native asset.
    /// </summary>
    [TestMethod]
    public void TestDeserializeValidJsonNative()
    {
        var json = @"{""asset"":""native"",""amount"":""250.50""}";

        var result = JsonSerializer.Deserialize<AssetAmount>(json, _options);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Asset, typeof(AssetTypeNative));
        Assert.AreEqual("250.50", result.Amount);
    }

    /// <summary>
    ///     Verifies deserialization of valid JSON with non-native asset.
    /// </summary>
    [TestMethod]
    public void TestDeserializeValidJsonNonNative()
    {
        var issuer = KeyPair.Random();
        var json = $@"{{""asset"":""USD:{issuer.AccountId}"",""amount"":""100.00""}}";

        var result = JsonSerializer.Deserialize<AssetAmount>(json, _options);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Asset, typeof(AssetTypeCreditAlphaNum));
        Assert.AreEqual("100.00", result.Amount);
    }

    /// <summary>
    ///     Verifies that deserialization throws ArgumentException when asset property is missing.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeMissingAssetThrowsException()
    {
        var json = @"{""amount"":""100""}";
        JsonSerializer.Deserialize<AssetAmount>(json, _options);
    }

    /// <summary>
    ///     Verifies that deserialization throws ArgumentException when amount property is missing.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeMissingAmountThrowsException()
    {
        var json = @"{""asset"":""native""}";
        JsonSerializer.Deserialize<AssetAmount>(json, _options);
    }

    /// <summary>
    ///     Verifies that deserialization throws ArgumentException when asset property is null.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeNullAssetThrowsException()
    {
        var json = @"{""asset"":null,""amount"":""100""}";
        JsonSerializer.Deserialize<AssetAmount>(json, _options);
    }

    /// <summary>
    ///     Verifies that deserialization throws ArgumentException when amount property is null.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeNullAmountThrowsException()
    {
        var json = @"{""asset"":""native"",""amount"":null}";
        JsonSerializer.Deserialize<AssetAmount>(json, _options);
    }

    /// <summary>
    ///     Verifies deserialization handles large amount values correctly.
    /// </summary>
    [TestMethod]
    public void TestDeserializeLargeAmount()
    {
        var json = @"{""asset"":""native"",""amount"":""922337203685477.5807""}";
        var result = JsonSerializer.Deserialize<AssetAmount>(json, _options);

        Assert.IsNotNull(result);
        Assert.AreEqual("922337203685477.5807", result.Amount);
    }

    /// <summary>
    ///     Verifies deserialization handles zero amount values correctly.
    /// </summary>
    [TestMethod]
    public void TestDeserializeZeroAmount()
    {
        var json = @"{""asset"":""native"",""amount"":""0""}";
        var result = JsonSerializer.Deserialize<AssetAmount>(json, _options);

        Assert.IsNotNull(result);
        Assert.AreEqual("0", result.Amount);
    }

    /// <summary>
    ///     Verifies serialization produces correct JSON format with asset and amount properties.
    /// </summary>
    [TestMethod]
    public void TestSerializeFormat()
    {
        var issuer = KeyPair.Random();
        var assetAmount = new AssetAmount(
            Asset.CreateNonNativeAsset("XLM", issuer.AccountId),
            "100.50"
        );

        var json = JsonSerializer.Serialize(assetAmount, _options);

        Assert.IsTrue(json.Contains("\"asset\":"));
        Assert.IsTrue(json.Contains("\"amount\":"));
        Assert.IsTrue(json.Contains($"XLM:{issuer.AccountId}"));
        Assert.IsTrue(json.Contains("100.50"));
    }

    /// <summary>
    ///     Verifies that deserialization throws JsonException for malformed JSON.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeInvalidJsonThrowsException()
    {
        // Missing closing brace
        var json = @"{""asset"":""native""";
        JsonSerializer.Deserialize<AssetAmount>(json, _options);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeWrongStructureThrowsException()
    {
        // Not JSON
        var json = @"""just a string""";
        JsonSerializer.Deserialize<AssetAmount>(json, _options);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeEmptyAssetThrowsException()
    {
        var json = @"{""asset"":"""",""amount"":""100""}";
        JsonSerializer.Deserialize<AssetAmount>(json, _options);
    }
}