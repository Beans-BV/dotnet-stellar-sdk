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
        Converters = { new LinkJsonConverter<Response>() },
    };

    /// <summary>
    ///     Tests deserialization of link JSON with only href property.
    ///     Verifies that links without templated property default to templated=false.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithHrefOnly_ReturnsLinkWithTemplatedFalse()
    {
        // Arrange
        var json = @"{""href"":""https://example.com""}";

        // Act
        var result = JsonSerializer.Deserialize<Link<Response>>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("https://example.com", result.Href);
        Assert.IsFalse(result.Templated);
    }

    /// <summary>
    ///     Tests deserialization of link JSON with templated=true property.
    ///     Verifies that templated property is correctly read from JSON.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithTemplatedTrue_ReturnsLinkWithTemplatedTrue()
    {
        // Arrange
        var json = @"{""href"":""https://example.com/{id}"",""templated"":true}";

        // Act
        var result = JsonSerializer.Deserialize<Link<Response>>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Templated);
    }

    /// <summary>
    ///     Tests serialization of link without templated flag set to true.
    ///     Verifies that templated property is omitted from JSON when false.
    /// </summary>
    [TestMethod]
    public void Serialize_WithoutTemplated_OmitsTemplatedProperty()
    {
        // Arrange
        var link = Link<Response>.Create("https://example.com", false);

        // Act
        var json = JsonSerializer.Serialize(link, _options);

        // Assert
        Assert.IsTrue(json.Contains("\"href\":\"https://example.com\""));
        Assert.IsFalse(json.Contains("templated"));
    }

    /// <summary>
    ///     Tests serialization of link with templated flag set to true.
    ///     Verifies that templated property is included in JSON when true.
    /// </summary>
    [TestMethod]
    public void Serialize_WithTemplatedTrue_IncludesTemplatedProperty()
    {
        // Arrange
        var link = Link<Response>.Create("https://example.com/{id}", true);

        // Act
        var json = JsonSerializer.Serialize(link, _options);

        // Assert
        Assert.IsTrue(json.Contains("\"templated\":true"));
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when JSON is not an object.
    ///     Verifies proper error handling for invalid JSON token types.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithNonObjectJson_ThrowsJsonException()
    {
        // Arrange
        var json = "\"string\"";

        // Act & Assert
        JsonSerializer.Deserialize<Link<Response>>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when href property is missing.
    ///     Verifies validation for required href property.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithMissingHref_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""templated"":true}";

        // Act & Assert
        JsonSerializer.Deserialize<Link<Response>>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when href property is null.
    ///     Verifies validation for non-null href property.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithNullHref_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""href"":null}";

        // Act & Assert
        JsonSerializer.Deserialize<Link<Response>>(json, _options);
    }
}