using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Converters;

public class AssetJsonConverter : JsonConverter<Asset>
{
    public override void WriteJson(JsonWriter writer, Asset? value, JsonSerializer serializer)
    {
        var jsonObject = new JObject();
        var assetType = new JProperty("asset_type", value?.Type);
        jsonObject.Add(assetType);
        if (value is AssetTypeCreditAlphaNum credit)
        {
            var code = new JProperty("asset_code", credit.Code);
            jsonObject.Add(code);
            var issuer = new JProperty("asset_issuer", credit.Issuer);
            jsonObject.Add(issuer);
        }

        jsonObject.WriteTo(writer);
    }

    public override Asset ReadJson(JsonReader reader, Type objectType, Asset? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var jt = JToken.ReadFrom(reader);
        var type = jt.Value<string>("asset_type");
        if (type == null) throw new ArgumentException("JSON value for asset_type is missing.", nameof(type));

        if (type == "native") return new AssetTypeNative();

        var code = jt.Value<string>("asset_code");
        if (code == null) throw new ArgumentException("JSON value for asset_code is missing.", nameof(code));
        var issuer = jt.Value<string>("asset_issuer");
        if (issuer == null) throw new ArgumentException("JSON value for asset_issuer is missing.", nameof(issuer));

        return Asset.CreateNonNativeAsset(code, issuer);
    }
}