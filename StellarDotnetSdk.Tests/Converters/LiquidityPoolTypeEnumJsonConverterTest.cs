using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Tests for LiquidityPoolTypeEnumJsonConverter JSON serialization and deserialization functionality.
/// </summary>
[TestClass]
public class LiquidityPoolTypeEnumJsonConverterTest
{
    private readonly JsonSerializerOptions _options = JsonOptions.DefaultOptions;

    /// <summary>
    ///     Verifies that constant product liquidity pool type serializes and deserializes correctly through JSON.
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithConstantProduct_RoundTripsCorrectly()
    {
        // Arrange
        var original = LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT;

        // Act
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);

        // Assert
        Assert.AreEqual(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, deserialized);
    }

    /// <summary>
    ///     Verifies that deserializing valid JSON string "constant_product" creates correct enum value.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithValidJsonConstantProduct_CreatesCorrectEnumValue()
    {
        // Arrange
        var json = @"""constant_product""";

        // Act
        var result = JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);

        // Assert
        Assert.AreEqual(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, result);
    }

    /// <summary>
    ///     Verifies that serializing constant product type produces correct JSON format.
    /// </summary>
    [TestMethod]
    public void Serialize_WithConstantProduct_ProducesCorrectJsonFormat()
    {
        // Arrange
        var poolType = LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT;

        // Act
        var json = JsonSerializer.Serialize(poolType, _options);

        // Assert
        Assert.AreEqual(@"""constant_product""", json);
    }

    /// <summary>
    ///     Verifies that deserializing JSON with unknown pool type throws JsonException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithUnknownType_ThrowsJsonException()
    {
        // Arrange
        var json = @"""unknown_pool_type""";

        // Act
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    /// <summary>
    ///     Verifies that deserializing JSON with empty string throws JsonException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithEmptyString_ThrowsJsonException()
    {
        // Arrange
        var json = @"""""";

        // Act
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    /// <summary>
    ///     Verifies that deserializing JSON with number token type throws JsonException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithNumberTokenType_ThrowsJsonException()
    {
        // Arrange
        var json = @"123";

        // Act
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    /// <summary>
    ///     Verifies that deserializing JSON with object token type throws JsonException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithObjectTokenType_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""type"":""constant_product""}";

        // Act
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    /// <summary>
    ///     Verifies that deserializing JSON with array token type throws JsonException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithArrayTokenType_ThrowsJsonException()
    {
        // Arrange
        var json = @"[""constant_product""]";

        // Act
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    /// <summary>
    ///     Verifies that deserializing invalid JSON throws JsonException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange - Missing closing quote
        var json = @"""constant_product";

        // Act
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    /// <summary>
    ///     Verifies that constant product type round-trips correctly through JSON with correct format.
    /// </summary>
    [TestMethod]
    public void RoundTrip_WithConstantProduct_RoundTripsCorrectly()
    {
        // Arrange
        var original = LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT;

        // Act
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);

        // Assert
        Assert.AreEqual(original, deserialized);
        Assert.AreEqual(@"""constant_product""", json);
    }

    /// <summary>
    ///     Verifies that deserializing JSON with incorrect case throws JsonException (case-sensitive).
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithIncorrectCase_ThrowsJsonException()
    {
        // Arrange
        var json = @"""CONSTANT_PRODUCT""";

        // Act
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    /// <summary>
    ///     Verifies that deserializing JSON with whitespace throws JsonException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithWhitespace_ThrowsJsonException()
    {
        // Arrange
        var json = @""" constant_product """;

        // Act
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    /// <summary>
    ///     Tests that serialization throws JsonException for unknown enum values.
    ///     Verifies proper error handling when enum value is not recognized (covers default case in Write method).
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Serialize_WithUnknownEnumValue_ThrowsJsonException()
    {
        // Arrange - Cast to unknown enum value to test the default case in Write method
        var unknownValue = (LiquidityPoolType.LiquidityPoolTypeEnum)999;

        // Act & Assert
        JsonSerializer.Serialize(unknownValue, _options);
    }
}