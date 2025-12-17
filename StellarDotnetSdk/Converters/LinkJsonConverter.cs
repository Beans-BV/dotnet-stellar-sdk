using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     JSON converter for Link&lt;T&gt; objects used in <a href="https://en.wikipedia.org/wiki/HATEOAS">HATEOAS</a>
///     responses.
/// </summary>
/// <typeparam name="T">The response type this link refers to</typeparam>
/// <remarks>
///     Performance: Uses direct instantiation (no reflection) for Native AOT compatibility.
/// </remarks>
public class LinkJsonConverter<T> : JsonConverter<Link<T>> where T : Response
{
    public override Link<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Expect object
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected StartObject or Null token for Link but found {reader.TokenType}");
        }

        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        var jsonObject = jsonDocument.RootElement;

        // Read templated property (optional, defaults to false)
        var templated = jsonObject.TryGetProperty("templated", out var templatedElement)
                        && templatedElement.GetBoolean();

        // Read href property (required)
        if (!jsonObject.TryGetProperty("href", out var hrefElement))
        {
            throw new JsonException("Link object must have 'href' property");
        }

        var href = hrefElement.GetString();
        if (href == null)
        {
            throw new JsonException("Link 'href' property cannot be null");
        }

        // Direct instantiation (no reflection - AOT compatible)
        return Link<T>.Create(href, templated);
    }

    public override void Write(Utf8JsonWriter writer, Link<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("href", value.Href);
        if (value.Templated)
        {
            writer.WriteBoolean("templated", value.Templated);
        }
        writer.WriteEndObject();
    }
}