using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace stellar_dotnet_sdk.converters;

public class AssetAmountJsonConverter : JsonConverter<AssetAmount>
{
    public override AssetAmount ReadJson(JsonReader reader, Type objectType, AssetAmount? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var jt = JToken.ReadFrom(reader);
        // TODO Consider moving this portion to a helper
        var assetName = jt.Value<string>("asset");
        var asset = string.IsNullOrEmpty(assetName) ? null : Asset.Create(assetName);

        var amount = jt.Value<string>("amount");

        if (asset == null) throw new ArgumentException("JSON value for asset is missing.", nameof(asset));

        if (amount == null) throw new ArgumentException("JSON value for amount is missing.", nameof(amount));

        return new AssetAmount(asset, amount);
    }

    public override void WriteJson(JsonWriter writer, AssetAmount? value, JsonSerializer serializer)
    {
        var jo = new JObject();
        if (value?.Asset != null) jo.Add("asset", value.Asset.CanonicalName());
        if (value?.Amount != null) jo.Add("amount", value.Amount);
        jo.WriteTo(writer);
    }
}