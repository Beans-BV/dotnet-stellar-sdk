using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.LiquidityPool;

namespace StellarDotnetSdk.Converters;

public class LiquidityPoolIdJsonConverter : JsonConverter<LiquidityPoolID>
{
    public override LiquidityPoolID? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        var liquidityPoolId = reader.GetString();
        return liquidityPoolId is null ? null : new LiquidityPoolID(liquidityPoolId);
    }

    public override void Write(Utf8JsonWriter writer, LiquidityPoolID? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.ToString());
    }
}