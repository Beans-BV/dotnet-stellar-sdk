using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Converters;

public class ReserveJsonConverter : JsonConverter<Reserve>
{
    public override Reserve Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        string? assetName = null;
        string? amount = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();
                switch (propertyName)
                {
                    case "asset":
                        assetName = reader.GetString();
                        break;
                    case "amount":
                        amount = reader.GetString();
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