using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     JSON converter for AssetAmount.
///     Handles conversion between JSON objects and AssetAmount instances.
/// </summary>
public class AssetAmountJsonConverter : JsonConverter<AssetAmount>
{
    public override AssetAmount Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // AssetAmount is non-nullable, only check for expected token type
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException(
                $"Expected StartObject for {nameof(AssetAmount)} but found {reader.TokenType}. " +
                "AssetAmount must be a JSON object with 'asset' and 'amount' properties."
            );
        }

        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        var jsonObject = jsonDocument.RootElement;

        if (!jsonObject.TryGetProperty("asset", out var assetElement))
        {
            throw new ArgumentException("JSON value for asset is missing.", nameof(assetElement));
        }
        var assetName = assetElement.GetString();
        var asset = string.IsNullOrEmpty(assetName) ? null : Asset.Create(assetName);

        if (!jsonObject.TryGetProperty("amount", out var amountElement))
        {
            throw new ArgumentException("JSON value for amount is missing.", nameof(amountElement));
        }
        var amount = amountElement.GetString();

        if (asset == null)
        {
            throw new ArgumentException("JSON value for asset is missing.", nameof(asset));
        }
        if (amount == null)
        {
            throw new ArgumentException("JSON value for amount is missing.", nameof(amount));
        }

        return new AssetAmount(asset, amount);
    }

    public override void Write(Utf8JsonWriter writer, AssetAmount value, JsonSerializerOptions options)
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