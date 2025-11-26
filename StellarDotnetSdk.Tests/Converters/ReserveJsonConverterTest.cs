using System;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Converters;

[TestClass]
public class ReserveJsonConverterTest
{
    private readonly JsonSerializerOptions _options = JsonOptions.DefaultOptions;

    [TestMethod]
    public void TestSerializeDeserializeNativeAsset()
    {
        var original = new Reserve
        {
            Asset = new AssetTypeNative(),
            Amount = "100.50"
        };

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Reserve>(json, _options);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("native", deserialized.Asset.CanonicalName());
        Assert.AreEqual("100.50", deserialized.Amount);
    }

    [TestMethod]
    public void TestSerializeDeserializeCreditAssetAlphaNum4()
    {
        var issuer = KeyPair.Random();
        var original = new Reserve
        {
            Asset = Asset.CreateNonNativeAsset("USD", issuer.AccountId),
            Amount = "500.0000000"
        };

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Reserve>(json, _options);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual($"USD:{issuer.AccountId}", deserialized.Asset.CanonicalName());
        Assert.AreEqual("500.0000000", deserialized.Amount);
    }

    [TestMethod]
    public void TestSerializeDeserializeCreditAssetAlphaNum12()
    {
        var issuer = KeyPair.Random();
        var original = new Reserve
        {
            Asset = Asset.CreateNonNativeAsset("LONGASSET123", issuer.AccountId),
            Amount = "1000.00"
        };

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Reserve>(json, _options);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual($"LONGASSET123:{issuer.AccountId}", deserialized.Asset.CanonicalName());
        Assert.AreEqual("1000.00", deserialized.Amount);
    }

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

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeMissingAssetThrowsException()
    {
        var json = @"{""amount"":""100""}";
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeMissingAmountThrowsException()
    {
        var json = @"{""asset"":""native""}";
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeNullAssetThrowsException()
    {
        var json = @"{""asset"":null,""amount"":""100""}";
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeNullAmountThrowsException()
    {
        var json = @"{""asset"":""native"",""amount"":null}";
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    [TestMethod]
    public void TestDeserializeLargeAmount()
    {
        var json = @"{""asset"":""native"",""amount"":""922337203685477.5807""}";
        var result = JsonSerializer.Deserialize<Reserve>(json, _options);

        Assert.IsNotNull(result);
        Assert.AreEqual("922337203685477.5807", result.Amount);
    }

    [TestMethod]
    public void TestDeserializeZeroAmount()
    {
        var json = @"{""asset"":""native"",""amount"":""0""}";
        var result = JsonSerializer.Deserialize<Reserve>(json, _options);

        Assert.IsNotNull(result);
        Assert.AreEqual("0", result.Amount);
    }

    [TestMethod]
    public void TestSerializeFormat()
    {
        var issuer = KeyPair.Random();
        var reserve = new Reserve
        {
            Asset = Asset.CreateNonNativeAsset("XLM", issuer.AccountId),
            Amount = "100.50"
        };

        var json = JsonSerializer.Serialize(reserve, _options);

        Assert.IsTrue(json.Contains("\"asset\":"));
        Assert.IsTrue(json.Contains("\"amount\":"));
        Assert.IsTrue(json.Contains($"XLM:{issuer.AccountId}"));
        Assert.IsTrue(json.Contains("100.50"));
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeInvalidJsonThrowsException()
    {
        // Missing closing brace
        var json = @"{""asset"":""native""";
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeWrongStructureThrowsException()
    {
        // Not an object
        var json = @"""just a string""";
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeEmptyAssetThrowsException()
    {
        var json = @"{""asset"":"""",""amount"":""100""}";
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeEmptyAmountThrowsException()
    {
        var json = @"{""asset"":""native"",""amount"":""""}";
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

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