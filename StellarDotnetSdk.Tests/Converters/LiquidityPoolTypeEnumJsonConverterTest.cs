using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Tests for LiquidityPoolTypeEnumJsonConverter.
///     Focus: enum string conversion for liquidity pool types.
/// </summary>
[TestClass]
public class LiquidityPoolTypeEnumJsonConverterTest
{
    private readonly JsonSerializerOptions _options = JsonOptions.DefaultOptions;

    /// <summary>
    ///     Tests round-trip serialization and deserialization of constant_product liquidity pool type.
    ///     Verifies that enum value can be serialized and deserialized correctly.
    /// </summary>
    [TestMethod]
    public void TestSerializeDeserializeConstantProduct()
    {
        var original = LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT;

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);

        Assert.AreEqual(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, deserialized);
    }

    /// <summary>
    ///     Tests deserialization of valid JSON string for constant_product pool type.
    ///     Verifies that "constant_product" string deserializes to correct enum value.
    /// </summary>
    [TestMethod]
    public void TestDeserializeValidJsonConstantProduct()
    {
        var json = @"""constant_product""";

        var result = JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);

        Assert.AreEqual(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, result);
    }

    /// <summary>
    ///     Tests serialization format of constant_product liquidity pool type.
    ///     Verifies that enum value serializes to correct JSON string format.
    /// </summary>
    [TestMethod]
    public void TestSerializeConstantProductFormat()
    {
        var poolType = LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT;

        var json = JsonSerializer.Serialize(poolType, _options);

        Assert.AreEqual(@"""constant_product""", json);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException for unknown pool type string.
    ///     Verifies proper error handling for invalid enum string values.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeUnknownTypeThrowsException()
    {
        var json = @"""unknown_pool_type""";
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException for empty string.
    ///     Verifies validation for non-empty string values.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeEmptyStringThrowsException()
    {
        var json = @"""""";
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when JSON token is a number.
    ///     Verifies proper error handling for invalid JSON token types.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeInvalidTokenTypeNumberThrowsException()
    {
        var json = @"123";
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when JSON token is an object.
    ///     Verifies proper error handling for invalid JSON token types.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeInvalidTokenTypeObjectThrowsException()
    {
        var json = @"{""type"":""constant_product""}";
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when JSON token is an array.
    ///     Verifies proper error handling for invalid JSON token types.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeInvalidTokenTypeArrayThrowsException()
    {
        var json = @"[""constant_product""]";
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException for malformed JSON.
    ///     Verifies proper error handling for invalid JSON syntax.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeInvalidJsonThrowsException()
    {
        // Missing closing quote
        var json = @"""constant_product";
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    /// <summary>
    ///     Tests round-trip serialization and deserialization of constant_product pool type.
    ///     Verifies that serialized enum can be deserialized back to the same value with correct JSON format.
    /// </summary>
    [TestMethod]
    public void TestRoundTripConstantProduct()
    {
        var original = LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT;

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);

        Assert.AreEqual(original, deserialized);
        Assert.AreEqual(@"""constant_product""", json);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException for case-sensitive mismatches.
    ///     Verifies that enum string matching is case-sensitive.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeCaseSensitiveThrowsException()
    {
        var json = @"""CONSTANT_PRODUCT""";
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException for strings with whitespace.
    ///     Verifies that enum string matching does not trim whitespace.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeWithWhitespaceThrowsException()
    {
        var json = @""" constant_product """;
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    /// <summary>
    ///     Tests that serialization throws JsonException for unknown enum values.
    ///     Verifies proper error handling when enum value is not recognized (covers default case in Write method).
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestSerializeUnknownEnumValueThrowsException()
    {
        // Cast to unknown enum value to test the default case in Write method
        var unknownValue = (LiquidityPoolType.LiquidityPoolTypeEnum)999;
        JsonSerializer.Serialize(unknownValue, _options);
    }
}