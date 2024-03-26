using System;
using Newtonsoft.Json;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Converters;

public class LiquidityPoolTypeEnumJsonConverter : JsonConverter<LiquidityPoolType.LiquidityPoolTypeEnum>
{
    public override LiquidityPoolType.LiquidityPoolTypeEnum ReadJson(JsonReader reader, Type objectType,
        LiquidityPoolType.LiquidityPoolTypeEnum existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return reader.Value switch
        {
            "constant_product" => LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
            _ => throw new Exception("type is not readable")
        };
    }

    public override void WriteJson(JsonWriter writer, LiquidityPoolType.LiquidityPoolTypeEnum value,
        JsonSerializer serializer)
    {
        switch (value)
        {
            case LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT:
                writer.WriteValue("constant_product");
                break;

            default:
                throw new Exception("type is not readable");
        }
    }
}