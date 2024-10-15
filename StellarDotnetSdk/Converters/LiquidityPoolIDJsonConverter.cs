using System;
using Newtonsoft.Json;
using StellarDotnetSdk.LiquidityPool;

namespace StellarDotnetSdk.Converters;

public class LiquidityPoolIdJsonConverter : JsonConverter<LiquidityPoolId>
{
    public override LiquidityPoolId? ReadJson(JsonReader reader, Type objectType, LiquidityPoolId? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        return reader.Value != null ? new LiquidityPoolId((string)reader.Value) : null;
    }

    public override void WriteJson(JsonWriter writer, LiquidityPoolId? value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.ToString());
    }
}