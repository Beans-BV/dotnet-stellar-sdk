using System;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Effects;

namespace StellarDotnetSdk.Tests.Converters;

[TestClass]
public class LiquidityPoolClaimableAssetAmountJsonConverterTest
{
    private readonly JsonSerializerOptions _options = JsonOptions.DefaultOptions;

    [TestMethod]
    public void TestSerializeDeserializeWithClaimableBalanceId()
    {
        var issuer = KeyPair.Random();
        var original = new LiquidityPoolClaimableAssetAmount
        {
            Asset = Asset.CreateNonNativeAsset("USD", issuer.AccountId),
            Amount = "100.50",
            ClaimableBalanceId = "00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072"
        };

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual($"USD:{issuer.AccountId}", deserialized.Asset.CanonicalName());
        Assert.AreEqual("100.50", deserialized.Amount);
        Assert.AreEqual("00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072",
            deserialized.ClaimableBalanceId);
    }

    [TestMethod]
    public void TestSerializeDeserializeWithoutClaimableBalanceId()
    {
        var issuer = KeyPair.Random();
        var original = new LiquidityPoolClaimableAssetAmount
        {
            Asset = new AssetTypeNative(),
            Amount = "250.00",
            ClaimableBalanceId = null
        };

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("native", deserialized.Asset.CanonicalName());
        Assert.AreEqual("250.00", deserialized.Amount);
        Assert.IsNull(deserialized.ClaimableBalanceId);
    }

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

    [TestMethod]
    public void TestSerializeFormat()
    {
        var issuer = KeyPair.Random();
        var obj = new LiquidityPoolClaimableAssetAmount
        {
            Asset = Asset.CreateNonNativeAsset("EUR", issuer.AccountId),
            Amount = "999.99",
            ClaimableBalanceId = "00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072"
        };

        var json = JsonSerializer.Serialize(obj, _options);

        Assert.IsTrue(json.Contains("\"asset\":"));
        Assert.IsTrue(json.Contains("\"amount\":"));
        Assert.IsTrue(json.Contains("\"claimable_balance_id\":"));
        Assert.IsTrue(json.Contains($"EUR:{issuer.AccountId}"));
        Assert.IsTrue(json.Contains("999.99"));
        Assert.IsTrue(json.Contains("00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072"));
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeInvalidJsonThrowsException()
    {
        // Incomplete JSON
        var json = @"{""asset"":""native"",""amount"":";
        JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeWrongStructureThrowsException()
    {
        // Not an object
        var json = @"""just a string""";
        JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);
    }

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