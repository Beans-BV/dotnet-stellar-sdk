using System;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Effects;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Tests for LiquidityPoolClaimableAssetAmountJsonConverter.
///     Focus: serialization and deserialization of LiquidityPoolClaimableAssetAmount with optional claimable_balance_id.
/// </summary>
[TestClass]
public class LiquidityPoolClaimableAssetAmountJsonConverterTest
{
    private readonly JsonSerializerOptions _options = JsonOptions.DefaultOptions;

    /// <summary>
    ///     Tests round-trip serialization and deserialization with claimable_balance_id present.
    ///     Verifies that claimable_balance_id is preserved through serialization cycle.
    /// </summary>
    [TestMethod]
    public void TestSerializeDeserializeWithClaimableBalanceId()
    {
        var issuer = KeyPair.Random();
        var original = new LiquidityPoolClaimableAssetAmount
        {
            Asset = Asset.CreateNonNativeAsset("USD", issuer.AccountId),
            Amount = "100.50",
            ClaimableBalanceId = "00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072",
        };

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual($"USD:{issuer.AccountId}", deserialized.Asset.CanonicalName());
        Assert.AreEqual("100.50", deserialized.Amount);
        Assert.AreEqual("00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072",
            deserialized.ClaimableBalanceId);
    }

    /// <summary>
    ///     Tests round-trip serialization and deserialization without claimable_balance_id.
    ///     Verifies that null claimable_balance_id is handled correctly.
    /// </summary>
    [TestMethod]
    public void TestSerializeDeserializeWithoutClaimableBalanceId()
    {
        var issuer = KeyPair.Random();
        var original = new LiquidityPoolClaimableAssetAmount
        {
            Asset = new AssetTypeNative(),
            Amount = "250.00",
            ClaimableBalanceId = null,
        };

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("native", deserialized.Asset.CanonicalName());
        Assert.AreEqual("250.00", deserialized.Amount);
        Assert.IsNull(deserialized.ClaimableBalanceId);
    }

    /// <summary>
    ///     Tests deserialization of valid JSON with claimable_balance_id property.
    ///     Verifies that claimable_balance_id is correctly deserialized from JSON.
    /// </summary>
    [TestMethod]
    public void TestDeserializeValidJsonWithClaimableBalanceId()
    {
        var issuer = KeyPair.Random();
        var json = $@"{{
            ""asset"":""USD:{issuer.AccountId}"",
            ""amount"":""500.0000000"",
            ""claimable_balance_id"":""00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072""
        }}";

        var result = JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Asset, typeof(AssetTypeCreditAlphaNum));
        Assert.AreEqual("500.0000000", result.Amount);
        Assert.AreEqual("00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072",
            result.ClaimableBalanceId);
    }

    /// <summary>
    ///     Tests deserialization of valid JSON with null claimable_balance_id property.
    ///     Verifies that explicit null claimable_balance_id is handled correctly.
    /// </summary>
    [TestMethod]
    public void TestDeserializeValidJsonNullClaimableBalanceId()
    {
        var json = @"{
            ""asset"":""native"",
            ""amount"":""100.00"",
            ""claimable_balance_id"":null
        }";

        var result = JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Asset, typeof(AssetTypeNative));
        Assert.AreEqual("100.00", result.Amount);
        Assert.IsNull(result.ClaimableBalanceId);
    }

    /// <summary>
    ///     Tests deserialization of valid JSON without claimable_balance_id property.
    ///     Verifies that missing claimable_balance_id defaults to null.
    /// </summary>
    [TestMethod]
    public void TestDeserializeValidJsonMissingClaimableBalanceId()
    {
        var json = @"{
            ""asset"":""native"",
            ""amount"":""75.25""
        }";

        var result = JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Asset, typeof(AssetTypeNative));
        Assert.AreEqual("75.25", result.Amount);
        Assert.IsNull(result.ClaimableBalanceId);
    }

    /// <summary>
    ///     Tests that deserialization throws ArgumentException when asset property is missing.
    ///     Verifies validation for required asset property.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeMissingAssetThrowsException()
    {
        var json = @"{
            ""amount"":""100"",
            ""claimable_balance_id"":""00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072""
        }";
        JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws ArgumentException when amount property is missing.
    ///     Verifies validation for required amount property.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeMissingAmountThrowsException()
    {
        var json = @"{
            ""asset"":""native"",
            ""claimable_balance_id"":""00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072""
        }";
        JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws ArgumentException when asset property is null.
    ///     Verifies validation for non-null asset property.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeNullAssetThrowsException()
    {
        var json = @"{
            ""asset"":null,
            ""amount"":""100"",
            ""claimable_balance_id"":""00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072""
        }";
        JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws ArgumentException when amount property is null.
    ///     Verifies validation for non-null amount property.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDeserializeNullAmountThrowsException()
    {
        var json = @"{
            ""asset"":""native"",
            ""amount"":null,
            ""claimable_balance_id"":""00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072""
        }";
        JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);
    }

    /// <summary>
    ///     Tests serialization produces correct JSON format with all properties.
    ///     Verifies that serialized JSON contains asset, amount, and claimable_balance_id properties.
    /// </summary>
    [TestMethod]
    public void TestSerializeFormat()
    {
        var issuer = KeyPair.Random();
        var obj = new LiquidityPoolClaimableAssetAmount
        {
            Asset = Asset.CreateNonNativeAsset("EUR", issuer.AccountId),
            Amount = "999.99",
            ClaimableBalanceId = "00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072",
        };

        var json = JsonSerializer.Serialize(obj, _options);

        Assert.IsTrue(json.Contains("\"asset\":"));
        Assert.IsTrue(json.Contains("\"amount\":"));
        Assert.IsTrue(json.Contains("\"claimable_balance_id\":"));
        Assert.IsTrue(json.Contains($"EUR:{issuer.AccountId}"));
        Assert.IsTrue(json.Contains("999.99"));
        Assert.IsTrue(json.Contains("00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072"));
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException for malformed JSON.
    ///     Verifies proper error handling for invalid JSON syntax.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeInvalidJsonThrowsException()
    {
        // Incomplete JSON
        var json = @"{""asset"":""native"",""amount"":";
        JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);
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
        JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);
    }

    /// <summary>
    ///     Tests deserialization handles large amount values correctly.
    ///     Verifies that maximum precision amounts are preserved.
    /// </summary>
    [TestMethod]
    public void TestDeserializeLargeAmount()
    {
        var json = @"{
            ""asset"":""native"",
            ""amount"":""922337203685477.5807"",
            ""claimable_balance_id"":null
        }";
        var result = JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);

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
        var json = @"{
            ""asset"":""native"",
            ""amount"":""0"",
            ""claimable_balance_id"":null
        }";
        var result = JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);

        Assert.IsNotNull(result);
        Assert.AreEqual("0", result.Amount);
    }
}