using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Converters;

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
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
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