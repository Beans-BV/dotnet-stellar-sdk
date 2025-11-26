using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     JSON converter for LiquidityPoolType enum.
///     Handles conversion between JSON string representations and LiquidityPoolTypeEnum values.
/// </summary>
public class LiquidityPoolTypeEnumJsonConverter : JsonConverter<LiquidityPoolType.LiquidityPoolTypeEnum>
{
    public override LiquidityPoolType.LiquidityPoolTypeEnum Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException(
                $"Expected String for LiquidityPoolType but found {reader.TokenType}. " +
                "LiquidityPoolType must be a string value."
            );
        }

        var type = reader.GetString();
        return type switch
        {
            "constant_product" => LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
            _ => throw new JsonException(
                $"Unknown liquidity pool type: '{type}'. Expected 'constant_product'. " +
                "This may indicate a new pool type not supported by this SDK version."
            ),
        };
    }

    public override void Write(Utf8JsonWriter writer, LiquidityPoolType.LiquidityPoolTypeEnum value,
        JsonSerializerOptions options)
    {
        switch (value)
        {
            case LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT:
                writer.WriteStringValue("constant_product");
                break;

            default:
                throw new JsonException(
                    $"Unknown LiquidityPoolType enum value: {value}. " +
                    "Cannot serialize unknown liquidity pool type."
                );
        }
    }
}