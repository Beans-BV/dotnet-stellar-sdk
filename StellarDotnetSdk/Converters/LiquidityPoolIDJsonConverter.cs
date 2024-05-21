using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.LiquidityPool;

namespace StellarDotnetSdk.Converters;

public class LiquidityPoolIdJsonConverter : JsonConverter<LiquidityPoolId>
{
    public override LiquidityPoolId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        var liquidityPoolId = reader.GetString();
        return liquidityPoolId is null ? null : new LiquidityPoolId(liquidityPoolId);
    }

    public override void Write(Utf8JsonWriter writer, LiquidityPoolId? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.ToString());
    }
}