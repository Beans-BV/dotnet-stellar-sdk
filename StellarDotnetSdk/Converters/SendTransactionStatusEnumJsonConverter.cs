using System;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif
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
    private static readonly IReadOnlyDictionary<string, SendTransactionResponse.SendTransactionStatus> StatusByName =
        new Dictionary<string, SendTransactionResponse.SendTransactionStatus>(StringComparer.Ordinal)
        {
            ["PENDING"] = SendTransactionResponse.SendTransactionStatus.PENDING,
            ["TRY_AGAIN_LATER"] = SendTransactionResponse.SendTransactionStatus.TRY_AGAIN_LATER,
            ["DUPLICATE"] = SendTransactionResponse.SendTransactionStatus.DUPLICATE,
            ["ERROR"] = SendTransactionResponse.SendTransactionStatus.ERROR,
        }
#if NET8_0_OR_GREATER
        .ToFrozenDictionary(StringComparer.Ordinal);
#else
        ;
#endif

    /// <inheritdoc />
    /// <remarks>
    ///     Matching is case-sensitive and integers are not accepted, unlike the built-in
    ///     <see cref="JsonStringEnumConverter" />: Stellar RPC emits exactly these four uppercase literals, and
    ///     reading a bare number would map <c>0</c> to the first member (<c>PENDING</c>) — turning a malformed
    ///     status into a plausible one. This converter must stay registered ahead of
    ///     <see cref="JsonStringEnumConverter" /> in <see cref="JsonOptions.DefaultOptions" /> for that to hold.
    /// </remarks>
    /// <exception cref="JsonException">
    ///     Thrown when the JSON value is not a string, or is not one of the four status literals.
    /// </exception>
    public override SendTransactionResponse.SendTransactionStatus Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException(
                $"Expected a string value for {nameof(SendTransactionResponse.SendTransactionStatus)} but found " +
                $"{reader.TokenType}.");
        }

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