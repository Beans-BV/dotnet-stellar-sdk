using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     JSON converter for Asset.
///     Handles conversion between JSON objects and Asset instances (native or credit assets).
/// </summary>
public class AssetJsonConverter : JsonConverter<Asset>
{
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

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
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
                }
            }

            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
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