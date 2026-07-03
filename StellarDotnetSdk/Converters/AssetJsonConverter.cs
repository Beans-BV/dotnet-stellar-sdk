using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     JSON converter for Asset.
///     Handles conversion between JSON objects and Asset instances (native or credit assets).
/// </summary>
/// <remarks>
///     Duplicate JSON property names are always rejected with a <see cref="JsonException" />, matched
///     case-insensitively. This is intentional hardening for financial fields and does not honor the
///     <see cref="JsonSerializerOptions" /> passed to <see cref="Read" /> — neither
///     <see cref="JsonSerializerOptions.AllowDuplicateProperties" /> nor
///     <see cref="JsonSerializerOptions.PropertyNameCaseInsensitive" /> changes it.
/// </remarks>
public class AssetJsonConverter : JsonConverter<Asset>
{
    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Asset value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("asset_type", value?.Type);
        if (value is AssetTypeCreditAlphaNum credit)
        {
            writer.WriteString("asset_code", credit.Code);
            writer.WriteString("asset_issuer", credit.Issuer);
        }

        writer.WriteEndObject();
    }

    /// <inheritdoc />
    public override Asset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Asset is non-nullable, only check for expected token type
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException(
                $"Expected StartObject for {nameof(Asset)} but found {reader.TokenType}. " +
                "Asset must be a JSON object with 'asset_type', and optionally 'asset_code' and 'asset_issuer'."
            );
        }

        string? type = null;
        string? code = null;
        string? issuer = null;
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
                JsonDuplicatePropertyGuard.MarkSeen(seen, propertyName, nameof(Asset));
                reader.Read();
                switch (propertyName)
                {
                    case "asset_type":
                        type = reader.GetString();
                        break;
                    case "asset_code":
                        code = reader.GetString();
                        break;
                    case "asset_issuer":
                        issuer = reader.GetString();
                        break;
                    default:
                        // Skip the whole value: an unrecognized object/array value would otherwise be
                        // walked as if its keys were top-level Asset properties, corrupting the
                        // duplicate guard's seen-set and desynchronizing the reader.
                        reader.Skip();
                        break;
                }
            }
        }

        if (type == null)
        {
            throw new ArgumentException("JSON value for asset_type is missing.", nameof(type));
        }

        if (type == "native")
        {
            return new AssetTypeNative();
        }

        if (code == null)
        {
            throw new ArgumentException("JSON value for asset_code is missing.", nameof(code));
        }
        if (issuer == null)
        {
            throw new ArgumentException("JSON value for asset_issuer is missing.", nameof(issuer));
        }

        return Asset.CreateNonNativeAsset(code, issuer);
    }
}