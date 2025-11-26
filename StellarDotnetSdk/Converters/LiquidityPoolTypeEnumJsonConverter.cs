using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Converters;

public class LiquidityPoolTypeEnumJsonConverter : JsonConverter<LiquidityPoolType.LiquidityPoolTypeEnum>
{
    public override LiquidityPoolType.LiquidityPoolTypeEnum Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        var type = reader.GetString();
        return type switch
        {
            "constant_product" => LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
            _ => throw new Exception("Type is not readable"),
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
                throw new Exception("Type is not readable");
        }
    }
}