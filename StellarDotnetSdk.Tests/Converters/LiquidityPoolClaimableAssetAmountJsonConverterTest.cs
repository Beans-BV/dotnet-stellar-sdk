using System;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Effects;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Tests for LiquidityPoolClaimableAssetAmountJsonConverter JSON serialization and deserialization functionality.
/// </summary>
[TestClass]
public class LiquidityPoolClaimableAssetAmountJsonConverterTest
{
    private readonly JsonSerializerOptions _options = JsonOptions.DefaultOptions;

    /// <summary>
    ///     Verifies that instances with claimable balance ID serialize and deserialize correctly through JSON.
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithClaimableBalanceId_RoundTripsCorrectly()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var original = new LiquidityPoolClaimableAssetAmount
        {
            Asset = Asset.CreateNonNativeAsset("USD", issuer.AccountId),
            Amount = "100.50",
            ClaimableBalanceId = "00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072",
        };

        // Act
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.IsNotNull(deserialized.Asset);
        Assert.AreEqual($"USD:{issuer.AccountId}", deserialized.Asset.CanonicalName());
        Assert.AreEqual("100.50", deserialized.Amount);
        Assert.AreEqual("00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072",
            deserialized.ClaimableBalanceId);
    }

    /// <summary>
    ///     Verifies that instances without claimable balance ID serialize and deserialize correctly through JSON.
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithoutClaimableBalanceId_RoundTripsCorrectly()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var original = new LiquidityPoolClaimableAssetAmount
        {
            Asset = new AssetTypeNative(),
            Amount = "250.00",
            ClaimableBalanceId = null,
        };

        // Act
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.IsNotNull(deserialized.Asset);
        Assert.AreEqual("native", deserialized.Asset.CanonicalName());
        Assert.AreEqual("250.00", deserialized.Amount);
        Assert.IsNull(deserialized.ClaimableBalanceId);
    }

    /// <summary>
    ///     Verifies that deserializing valid JSON with claimable balance ID creates correct instance.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithValidJsonAndClaimableBalanceId_CreatesCorrectInstance()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var json = $@"{{
            ""asset"":""USD:{issuer.AccountId}"",
            ""amount"":""500.0000000"",
            ""claimable_balance_id"":""00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072""
        }}";

        // Act
        var result = JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Asset, typeof(AssetTypeCreditAlphaNum));
        Assert.AreEqual("500.0000000", result.Amount);
        Assert.AreEqual("00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072",
            result.ClaimableBalanceId);
    }

    /// <summary>
    ///     Verifies that deserializing valid JSON with null claimable balance ID creates correct instance.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithValidJsonAndNullClaimableBalanceId_CreatesCorrectInstance()
    {
        // Arrange
        var json = @"{
            ""asset"":""native"",
            ""amount"":""100.00"",
            ""claimable_balance_id"":null
        }";

        // Act
        var result = JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Asset, typeof(AssetTypeNative));
        Assert.AreEqual("100.00", result.Amount);
        Assert.IsNull(result.ClaimableBalanceId);
    }

    /// <summary>
    ///     Verifies that deserializing valid JSON without claimable balance ID property creates instance with null claimable
    ///     balance ID.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithValidJsonMissingClaimableBalanceId_CreatesInstanceWithNullClaimableBalanceId()
    {
        // Arrange
        var json = @"{
            ""asset"":""native"",
            ""amount"":""75.25""
        }";

        // Act
        var result = JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Asset, typeof(AssetTypeNative));
        Assert.AreEqual("75.25", result.Amount);
        Assert.IsNull(result.ClaimableBalanceId);
    }

    /// <summary>
    ///     Verifies that deserializing JSON with missing asset property throws ArgumentException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Deserialize_WithMissingAsset_ThrowsArgumentException()
    {
        // Arrange
        var json = @"{
            ""amount"":""100"",
            ""claimable_balance_id"":""00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072""
        }";

        // Act
        JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);
    }

    /// <summary>
    ///     Verifies that deserializing JSON with missing amount property throws ArgumentException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Deserialize_WithMissingAmount_ThrowsArgumentException()
    {
        // Arrange
        var json = @"{
            ""asset"":""native"",
            ""claimable_balance_id"":""00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072""
        }";

        // Act
        JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);
    }

    /// <summary>
    ///     Verifies that deserializing JSON with null asset property throws ArgumentException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Deserialize_WithNullAsset_ThrowsArgumentException()
    {
        // Arrange
        var json = @"{
            ""asset"":null,
            ""amount"":""100"",
            ""claimable_balance_id"":""00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072""
        }";

        // Act
        JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);
    }

    /// <summary>
    ///     Verifies that deserializing JSON with null amount property throws ArgumentException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Deserialize_WithNullAmount_ThrowsArgumentException()
    {
        // Arrange
        var json = @"{
            ""asset"":""native"",
            ""amount"":null,
            ""claimable_balance_id"":""00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072""
        }";

        // Act
        JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);
    }

    /// <summary>
    ///     Verifies that serialization produces JSON with correct property names and values.
    /// </summary>
    [TestMethod]
    public void Serialize_ProducesCorrectJsonFormat()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var obj = new LiquidityPoolClaimableAssetAmount
        {
            Asset = Asset.CreateNonNativeAsset("EUR", issuer.AccountId),
            Amount = "999.99",
            ClaimableBalanceId = "00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072",
        };

        // Act
        var json = JsonSerializer.Serialize(obj, _options);

        // Assert
        Assert.IsTrue(json.Contains("\"asset\":"));
        Assert.IsTrue(json.Contains("\"amount\":"));
        Assert.IsTrue(json.Contains("\"claimable_balance_id\":"));
        Assert.IsTrue(json.Contains($"EUR:{issuer.AccountId}"));
        Assert.IsTrue(json.Contains("999.99"));
        Assert.IsTrue(json.Contains("00000000929b20b72e5890ab51c24f1cc46fa01c4f318d8d33367d24dd614cfdf5491072"));
    }

    /// <summary>
    ///     Verifies that deserializing invalid JSON throws JsonException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange - Incomplete JSON
        var json = @"{""asset"":""native"",""amount"":";

        // Act
        JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);
    }

    /// <summary>
    ///     Verifies that deserializing JSON with wrong structure throws JsonException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithWrongStructure_ThrowsJsonException()
    {
        // Arrange - Not an object
        var json = @"""just a string""";

        // Act
        JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);
    }

    /// <summary>
    ///     Verifies that deserializing JSON with large amount value succeeds.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithLargeAmount_Succeeds()
    {
        // Arrange
        var json = @"{
            ""asset"":""native"",
            ""amount"":""922337203685477.5807"",
            ""claimable_balance_id"":null
        }";

        // Act
        var result = JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("922337203685477.5807", result.Amount);
    }

    /// <summary>
    ///     Verifies that deserializing JSON with zero amount value succeeds.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithZeroAmount_Succeeds()
    {
        // Arrange
        var json = @"{
            ""asset"":""native"",
            ""amount"":""0"",
            ""claimable_balance_id"":null
        }";

        // Act
        var result = JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("0", result.Amount);
    }
}