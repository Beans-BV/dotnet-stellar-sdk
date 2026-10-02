using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Unit tests for <see cref="NonNullElementListJsonConverter{T}" />, which rejects a <c>null</c> list element.
/// </summary>
[TestClass]
public class NonNullElementListJsonConverterTest
{
    private sealed class Item
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }

    private sealed class Holder
    {
        [JsonConverter(typeof(NonNullElementListJsonConverter<Item>))]
        [JsonPropertyName("items")]
        public IReadOnlyList<Item>? Items { get; init; }
    }

    [TestMethod]
    public void Read_AttachedToAProperty_RejectsANullElement()
    {
        var ex = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<Holder>("{\"items\":[{\"name\":\"a\"},null]}"));

        StringAssert.Contains(ex.Message, "null element at index 1");
    }

    [TestMethod]
    [DataRow("{\"items\":\"x\"}")]
    [DataRow("{\"items\":{}}")]
    [DataRow("{\"items\":1}")]
    public void Read_WhenTheValueIsNotAnArray_ThrowsJsonException(string json)
    {
        var ex = Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<Holder>(json));

        StringAssert.Contains(ex.Message, "Expected a JSON array");
    }

    [TestMethod]
    public void Read_WhenAnElementFails_ReportsItsIndexAndTheAbsolutePath()
    {
        var ex = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<Holder>("{\"items\":[{\"name\":\"a\"},{\"name\":42}]}"));

        StringAssert.Contains(ex.Message, "element at index 1");
        StringAssert.StartsWith(ex.Path, "$.items");
    }

    /// <summary>
    ///     A direct caller can hand the converter a reader over a partial buffer; the serializer never does. Running
    ///     out of data before the closing bracket must fail rather than return the elements read so far.
    /// </summary>
    [TestMethod]
    public void Read_CalledDirectlyOnATruncatedArray_ThrowsInsteadOfReturningAPartialList()
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes("[{\"name\":\"a\"},{\"name\":\"b\"}");
        var converter = new NonNullElementListJsonConverter<Item>();

        var ex = Assert.ThrowsException<JsonException>(() =>
        {
            var reader = new Utf8JsonReader(bytes, false, default);
            reader.Read();
            return converter.Read(ref reader, typeof(IReadOnlyList<Item>), new JsonSerializerOptions());
        });

        StringAssert.Contains(ex.Message, "ended before");
    }

    [TestMethod]
    public void Read_ReturnsTheElementsInOrder()
    {
        var holder = JsonSerializer.Deserialize<Holder>("{\"items\":[{\"name\":\"a\"},{\"name\":\"b\"}],\"after\":1}")!;

        CollectionAssert.AreEqual(new[] { "a", "b" }, System.Linq.Enumerable.ToList(
            System.Linq.Enumerable.Select(holder.Items!, i => i.Name)));
    }

    [TestMethod]
    public void Write_WithANullElement_ThrowsBeforeWritingTheList()
    {
        var holder = new Holder { Items = new[] { new Item { Name = "a" }, null! } };

        var ex = Assert.ThrowsException<JsonException>(() => JsonSerializer.Serialize(holder));

        StringAssert.Contains(ex.Message, "null element at index 1");
    }

    /// <summary>
    ///     Pins the documented reason this converter, unlike <see cref="NonNullElementArrayJsonConverter{T}" />,
    ///     has no global-registration guard: it delegates for other types than the one it converts, so a global
    ///     registration cannot recurse. Were it to delegate for <see cref="IReadOnlyList{T}" />, this test would
    ///     terminate the test host with a <c>StackOverflowException</c> rather than pass.
    /// </summary>
    [TestMethod]
    public void RegisteredGlobally_ReadsAndWritesWithoutRecursing()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new NonNullElementListJsonConverter<Item>());

        var list = JsonSerializer.Deserialize<IReadOnlyList<Item>>("[{\"name\":\"a\"},{\"name\":\"b\"}]", options)!;

        Assert.AreEqual(2, list.Count);
        Assert.AreEqual("[{\"name\":\"a\"},{\"name\":\"b\"}]", JsonSerializer.Serialize(list, options));
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<IReadOnlyList<Item>>("[null]", options));
    }
}
