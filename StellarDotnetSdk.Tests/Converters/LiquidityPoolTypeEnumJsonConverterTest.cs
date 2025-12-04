using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Converters;

[TestClass]
public class LiquidityPoolTypeEnumJsonConverterTest
{
    private readonly JsonSerializerOptions _options = JsonOptions.DefaultOptions;

    [TestMethod]
    public void TestSerializeDeserializeConstantProduct()
    {
        var original = LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT;

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);

        Assert.AreEqual(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, deserialized);
    }

    [TestMethod]
    public void TestDeserializeValidJsonConstantProduct()
    {
        var json = @"""constant_product""";

        var result = JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);

        Assert.AreEqual(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, result);
    }

    [TestMethod]
    public void TestSerializeConstantProductFormat()
    {
        var poolType = LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT;

        var json = JsonSerializer.Serialize(poolType, _options);

        Assert.AreEqual(@"""constant_product""", json);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeUnknownTypeThrowsException()
    {
        var json = @"""unknown_pool_type""";
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeEmptyStringThrowsException()
    {
        var json = @"""""";
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeInvalidTokenTypeNumberThrowsException()
    {
        var json = @"123";
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeInvalidTokenTypeObjectThrowsException()
    {
        var json = @"{""type"":""constant_product""}";
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeInvalidTokenTypeArrayThrowsException()
    {
        var json = @"[""constant_product""]";
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeInvalidJsonThrowsException()
    {
        // Missing closing quote
        var json = @"""constant_product";
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    [TestMethod]
    public void TestRoundTripConstantProduct()
    {
        var original = LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT;

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);

        Assert.AreEqual(original, deserialized);
        Assert.AreEqual(@"""constant_product""", json);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeCaseSensitiveThrowsException()
    {
        var json = @"""CONSTANT_PRODUCT""";
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestDeserializeWithWhitespaceThrowsException()
    {
        var json = @""" constant_product """;
        JsonSerializer.Deserialize<LiquidityPoolType.LiquidityPoolTypeEnum>(json, _options);
    }
}