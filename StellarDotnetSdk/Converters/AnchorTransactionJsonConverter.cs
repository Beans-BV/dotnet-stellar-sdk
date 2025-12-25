using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Sep.Sep0006.Responses;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     JSON converter for AnchorTransaction that handles the nested "transaction" key
///     in required_info_updates field, which is sometimes wrapped in an extra object.
/// </summary>
public class AnchorTransactionJsonConverter : JsonConverter<AnchorTransaction>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(AnchorTransaction);
    }

    public override AnchorTransaction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        // Handle nested "transaction" key in required_info_updates
        if (root.TryGetProperty("required_info_updates", out var requiredInfoUpdatesElement) &&
            requiredInfoUpdatesElement.ValueKind == JsonValueKind.Object &&
            requiredInfoUpdatesElement.TryGetProperty("transaction", out var transactionElement) &&
            transactionElement.ValueKind == JsonValueKind.Object)
        {
            // Unwrap the nested structure by replacing required_info_updates with the transaction object
            using var writer = new MemoryStream();
            using var jsonWriter = new Utf8JsonWriter(writer);
            jsonWriter.WriteStartObject();

            foreach (var property in root.EnumerateObject())
            {
                if (property.Name == "required_info_updates")
                {
                    jsonWriter.WritePropertyName("required_info_updates");
                    transactionElement.WriteTo(jsonWriter);
                }
                else
                {
                    property.WriteTo(jsonWriter);
                }
            }

            jsonWriter.WriteEndObject();
            jsonWriter.Flush();

            var modifiedJson = Encoding.UTF8.GetString(writer.ToArray());
            var anchorTransaction = JsonSerializer.Deserialize<AnchorTransaction>(modifiedJson, options);
            if (anchorTransaction is null)
            {
                throw new JsonException("Failed to deserialize AnchorTransaction from modified JSON.");
            }
            return anchorTransaction;
        }
        // Normal deserialization
        var normalAnchorTransaction = JsonSerializer.Deserialize<AnchorTransaction>(root.GetRawText(), options);
        if (normalAnchorTransaction is null)
        {
            throw new JsonException("Failed to deserialize AnchorTransaction.");
        }
        return normalAnchorTransaction;
    }

    public override void Write(Utf8JsonWriter writer, AnchorTransaction value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}