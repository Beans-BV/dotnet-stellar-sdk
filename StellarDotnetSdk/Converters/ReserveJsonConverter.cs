using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     JSON converter for Reserve.
///     Handles conversion between JSON objects and Reserve instances.
/// </summary>
/// <remarks>
///     Duplicate JSON property names are always rejected with a <see cref="JsonException" />, matched
///     case-insensitively. This is intentional hardening for financial fields and does not honor the
///     <see cref="JsonSerializerOptions" /> passed to <see cref="Read" /> — neither
///     <see cref="JsonSerializerOptions.AllowDuplicateProperties" /> nor
///     <see cref="JsonSerializerOptions.PropertyNameCaseInsensitive" /> changes it.
/// </remarks>
public class ReserveJsonConverter : JsonConverter<Reserve>
{
    /// <inheritdoc />
    public override Reserve Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Reserve is non-nullable, only check for expected token type
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException(
                $"Expected StartObject for {nameof(Reserve)} but found {reader.TokenType}. " +
                "Reserve must be a JSON object with 'asset' and 'amount' properties."
            );
        }

        string? assetName = null;
        string? amount = null;
        var seen = JsonDuplicatePropertyGuard.CreateSeenSet();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString()!;
                JsonDuplicatePropertyGuard.MarkSeen(seen, propertyName, nameof(Reserve));
                reader.Read();
                switch (propertyName)
                {
                    case "asset":
                        assetName = reader.GetString();
                        break;
                    case "amount":
                        amount = reader.GetString();
                        break;
                    default:
                        // Skip the whole value: an unrecognized object/array value would otherwise be
                        // walked as if its keys were top-level Reserve properties, corrupting the
                        // duplicate guard's seen-set and desynchronizing the reader.
                        reader.Skip();
                        break;
                }
            }
        }

        if (string.IsNullOrEmpty(assetName))
        {
            throw new ArgumentException("JSON value for asset is missing.", nameof(assetName));
        }

        if (string.IsNullOrEmpty(amount))
        {
            throw new ArgumentException("JSON value for amount is missing.", nameof(amount));
        }

        var asset = Asset.Create(assetName);

        return new Reserve
        {
            Amount = amount,
            Asset = asset,
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Reserve value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value.Asset != null)
        {
            writer.WriteString("asset", value.Asset.CanonicalName());
        }
        if (value.Amount != null)
        {
            writer.WriteString("amount", value.Amount);
        }
        writer.WriteEndObject();
    }
}