using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     JSON converter for LiquidityPoolType enum.
///     Handles conversion between JSON string representations and LiquidityPoolTypeEnum values.
/// </summary>
/// <remarks>
///     Performance: Uses a <see cref="FrozenDictionary{TKey,TValue}" /> for both string→enum and
///     enum→string dispatch, yielding faster reads than a switch expression on immutable data.
/// </remarks>
public class LiquidityPoolTypeEnumJsonConverter : JsonConverter<LiquidityPoolType.LiquidityPoolTypeEnum>
{
    /// <summary>
    ///     Frozen lookup table mapping the Horizon wire-format strings to <see cref="LiquidityPoolType.LiquidityPoolTypeEnum" />
    ///     values.
    /// </summary>
    private static readonly FrozenDictionary<string, LiquidityPoolType.LiquidityPoolTypeEnum> EnumByName =
        new Dictionary<string, LiquidityPoolType.LiquidityPoolTypeEnum>(StringComparer.Ordinal)
        {
            ["constant_product"] = LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
        }.ToFrozenDictionary(StringComparer.Ordinal);

    /// <summary>
    ///     Frozen reverse lookup table mapping <see cref="LiquidityPoolType.LiquidityPoolTypeEnum" /> values to their
    ///     Horizon wire-format strings.
    /// </summary>
    private static readonly FrozenDictionary<LiquidityPoolType.LiquidityPoolTypeEnum, string> NameByEnum =
        new Dictionary<LiquidityPoolType.LiquidityPoolTypeEnum, string>
        {
            [LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT] = "constant_product",
        }.ToFrozenDictionary();

    /// <inheritdoc />
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
        if (type != null && EnumByName.TryGetValue(type, out var value))
        {
            return value;
        }

        throw new JsonException(
            $"Unknown liquidity pool type: '{type}'. Expected 'constant_product'. " +
            "This may indicate a new pool type not supported by this SDK version."
        );
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, LiquidityPoolType.LiquidityPoolTypeEnum value,
        JsonSerializerOptions options)
    {
        if (!NameByEnum.TryGetValue(value, out var name))
        {
            throw new JsonException(
                $"Unknown LiquidityPoolType enum value: {value}. " +
                "Cannot serialize unknown liquidity pool type."
            );
        }

        writer.WriteStringValue(name);
    }
}
