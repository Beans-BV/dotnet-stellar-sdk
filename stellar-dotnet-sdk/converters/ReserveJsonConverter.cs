using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace stellar_dotnet_sdk.converters;

public class ReserveJsonConverter : JsonConverter<Reserve>
{
    public override Reserve ReadJson(JsonReader reader, Type objectType, Reserve? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var jt = JToken.ReadFrom(reader);

        var assetName = jt.Value<string>("asset");
        var asset = string.IsNullOrEmpty(assetName) ? null : Asset.Create(assetName);

        var amount = jt.Value<string>("amount");

        if (asset == null) throw new ArgumentException("JSON value for asset is missing.", nameof(asset));

        if (amount == null) throw new ArgumentException("JSON value for amount is missing.", nameof(amount));

        return new Reserve(amount, asset);
    }

    public override void WriteJson(JsonWriter writer, Reserve? value, JsonSerializer serializer)
    {
        var jo = new JObject();
        if (value?.Asset != null) jo.Add("asset", value.Asset.CanonicalName());
        if (value?.Amount != null) jo.Add("amount", value.Amount);
        jo.WriteTo(writer);
    }
}