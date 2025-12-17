using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Tests for LinkJsonConverter.
///     Focus: HATEOAS link deserialization with href and optional templated flag.
/// </summary>
[TestClass]
public class LinkJsonConverterTest
{
    private readonly JsonSerializerOptions _options = new()
    {
        Converters = { new LinkJsonConverter<Response>() }
    };

    /// <summary>
    ///     Tests deserialization of link JSON with only href property.
    ///     Verifies that links without templated property default to templated=false.
    /// </summary>
    [TestMethod]
    public void TestRead_WithHrefOnly()
    {
        var result = JsonSerializer.Deserialize<Link<Response>>(@"{""href"":""https://example.com""}", _options);

        Assert.IsNotNull(result);
        Assert.AreEqual("https://example.com", result.Href);
        Assert.IsFalse(result.Templated);
    }

    /// <summary>
    ///     Tests deserialization of link JSON with templated=true property.
    ///     Verifies that templated property is correctly read from JSON.
    /// </summary>
    [TestMethod]
    public void TestRead_WithTemplatedTrue()
    {
        var result = JsonSerializer.Deserialize<Link<Response>>(@"{""href"":""https://example.com/{id}"",""templated"":true}", _options);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Templated);
    }

    /// <summary>
    ///     Tests serialization of link without templated flag set to true.
    ///     Verifies that templated property is omitted from JSON when false.
    /// </summary>
    [TestMethod]
    public void TestWrite_WithoutTemplated()
    {
        var link = Link<Response>.Create("https://example.com", false);

        var json = JsonSerializer.Serialize(link, _options);

        Assert.IsTrue(json.Contains("\"href\":\"https://example.com\""));
        Assert.IsFalse(json.Contains("templated"));
    }

    /// <summary>
    ///     Tests serialization of link with templated flag set to true.
    ///     Verifies that templated property is included in JSON when true.
    /// </summary>
    [TestMethod]
    public void TestWrite_WithTemplatedTrue()
    {
        var link = Link<Response>.Create("https://example.com/{id}", true);

        var json = JsonSerializer.Serialize(link, _options);

        Assert.IsTrue(json.Contains("\"templated\":true"));
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when JSON is not an object.
    ///     Verifies proper error handling for invalid JSON token types.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestRead_ThrowsForNonObject()
    {
        JsonSerializer.Deserialize<Link<Response>>("\"string\"", _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when href property is missing.
    ///     Verifies validation for required href property.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestRead_ThrowsForMissingHref()
    {
        JsonSerializer.Deserialize<Link<Response>>(@"{""templated"":true}", _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when href property is null.
    ///     Verifies validation for non-null href property.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestRead_ThrowsForNullHref()
    {
        JsonSerializer.Deserialize<Link<Response>>(@"{""href"":null}", _options);
    }
}
