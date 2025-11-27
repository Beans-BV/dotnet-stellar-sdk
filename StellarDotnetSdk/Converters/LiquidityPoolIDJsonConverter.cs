using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.LiquidityPool;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     JSON converter for LiquidityPoolId.
///     Handles conversion between JSON strings and LiquidityPoolId instances.
/// </summary>
public class LiquidityPoolIdJsonConverter : JsonConverter<LiquidityPoolId>
{
    public override LiquidityPoolId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Check for null token first
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        // Then check for expected token type
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException(
                $"Expected String or Null for {nameof(LiquidityPoolId)} but found {reader.TokenType}. " +
                "LiquidityPoolId must be a string value."
            );
        }

        var liquidityPoolId = reader.GetString();
        return liquidityPoolId is null ? null : new LiquidityPoolId(liquidityPoolId);
    }

    public override void Write(Utf8JsonWriter writer, LiquidityPoolId? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.ToString());
    }
}