using System;
using Newtonsoft.Json;

namespace StellarDotnetSdk.Converters;

public class LiquidityPoolIDJsonConverter : JsonConverter<LiquidityPoolID>
{
    public override LiquidityPoolID? ReadJson(JsonReader reader, Type objectType, LiquidityPoolID existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        return reader.Value != null ? new LiquidityPoolID((string)reader.Value) : null;
    }

    public override void WriteJson(JsonWriter writer, LiquidityPoolID? value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}