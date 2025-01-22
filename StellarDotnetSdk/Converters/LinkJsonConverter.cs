using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Converters
{
    public class LinkJsonConverter<T> : JsonConverter<Link<T>> where T : Response
    {
        public override Link<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            using var jsonDocument = JsonDocument.ParseValue(ref reader);
            var jsonObject = jsonDocument.RootElement;
            var templated = jsonObject.TryGetProperty("templated", out var templatedElement)
                ? templatedElement.GetBoolean()
                : false;
            var href = jsonObject.GetProperty("href").GetString();

            if (href == null)
                throw new JsonException();
            
            return (Link<T>)typeToConvert.GetMethod("Create").Invoke(null, [href, templated]);
        }

        public override void Write(Utf8JsonWriter writer, Link<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("href", value.Href);
            if (value.Templated)
                writer.WriteBoolean("templated", value.Templated);
            writer.WriteEndObject();
        }
    }
}