using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.SorobanRpc;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     JSON converter for <see cref="SendTransactionResponse.SendTransactionStatus" /> that maps between
///     the Soroban RPC string representations (e.g., "PENDING", "ERROR") and the corresponding enum values.
/// </summary>
/// <remarks>
///     Performance: Uses a <see cref="FrozenDictionary{TKey,TValue}" /> for string→enum dispatch,
///     yielding faster reads than a switch expression on immutable data.
/// </remarks>
public class SendTransactionStatusEnumJsonConverter : JsonConverter<SendTransactionResponse.SendTransactionStatus>
{
    /// <summary>
    ///     Frozen lookup table mapping the Soroban RPC wire-format status strings to enum values.
    /// </summary>
    private static readonly FrozenDictionary<string, SendTransactionResponse.SendTransactionStatus> StatusByName =
        new Dictionary<string, SendTransactionResponse.SendTransactionStatus>(StringComparer.Ordinal)
        {
            ["PENDING"] = SendTransactionResponse.SendTransactionStatus.PENDING,
            ["TRY_AGAIN_LATER"] = SendTransactionResponse.SendTransactionStatus.TRY_AGAIN_LATER,
            ["DUPLICATE"] = SendTransactionResponse.SendTransactionStatus.DUPLICATE,
            ["ERROR"] = SendTransactionResponse.SendTransactionStatus.ERROR,
        }.ToFrozenDictionary(StringComparer.Ordinal);

    /// <inheritdoc />
    public override SendTransactionResponse.SendTransactionStatus Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value != null && StatusByName.TryGetValue(value, out var status))
        {
            return status;
        }

        throw new JsonException(
            $"Value '{value}' cannot be converted to type {nameof(SendTransactionResponse.SendTransactionStatus)}.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SendTransactionResponse.SendTransactionStatus value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}