using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Converters
{
    public class AssetAmountJsonConverter : JsonConverter<AssetAmount>
    {
        public override AssetAmount Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            using (var jsonDocument = JsonDocument.ParseValue(ref reader))
            {
                var jsonObject = jsonDocument.RootElement;
                var assetName = jsonObject.GetProperty("asset").GetString();
                var asset = string.IsNullOrEmpty(assetName) ? null : Asset.Create(assetName);

                var amount = jsonObject.GetProperty("amount").GetString();

                if (asset == null) throw new ArgumentException("JSON value for asset is missing.", nameof(asset));
                if (amount == null) throw new ArgumentException("JSON value for amount is missing.", nameof(amount));

                return new AssetAmount(asset, amount);
            }
        }

        public override void Write(Utf8JsonWriter writer, AssetAmount value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            if (value.Asset != null) writer.WriteString("asset", value.Asset.CanonicalName());
            if (value.Amount != null) writer.WriteString("amount", value.Amount);
            writer.WriteEndObject();
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(AssetAmount);
        }
    }
}