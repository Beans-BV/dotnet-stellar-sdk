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
///     JSON converter for <see cref="TransactionInfo.TransactionStatus" /> that maps between the Stellar RPC
///     string representations ("NOT_FOUND", "SUCCESS", "FAILED") and the corresponding enum values.
/// </summary>
/// <remarks>
///     The sibling of <see cref="SendTransactionStatusEnumJsonConverter" />, for the same reason and with the same
///     registration requirement: it must stay ahead of <see cref="JsonStringEnumConverter" /> in
///     <see cref="JsonOptions.DefaultOptions" /> or the catch-all shadows it wherever the enum is deserialized
///     on its own. <see cref="TransactionInfo.Status" /> additionally applies this converter with a
///     property-level <see cref="JsonConverterAttribute" />, which System.Text.Json resolves ahead of any
///     options' converter collection — so the property path stays strict whichever
///     <see cref="JsonSerializerOptions" /> a consumer supplies.
///     <para>
///         The stakes here are higher than for <c>sendTransaction</c>. The standard converter accepts bare
///         integers and maps them by ordinal, so <c>"status": 1</c> read as
///         <see cref="TransactionInfo.TransactionStatus.SUCCESS" /> — on the endpoint callers poll to decide
///         whether a payment settled. It is also case-insensitive, and it happily produces undefined values: a
///         response of <c>"status": 7</c> yielded the enum value <c>7</c>, which matches none of the three members
///         and so silently fails every comparison a caller writes.
///     </para>
/// </remarks>
public class TransactionStatusJsonConverter : JsonConverter<TransactionInfo.TransactionStatus>
{
    /// <summary>
    ///     Frozen lookup table mapping the Stellar RPC wire-format status strings to enum values.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, TransactionInfo.TransactionStatus> StatusByName =
        new Dictionary<string, TransactionInfo.TransactionStatus>(StringComparer.Ordinal)
        {
            ["NOT_FOUND"] = TransactionInfo.TransactionStatus.NOT_FOUND,
            ["SUCCESS"] = TransactionInfo.TransactionStatus.SUCCESS,
            ["FAILED"] = TransactionInfo.TransactionStatus.FAILED,
        }
#if NET8_0_OR_GREATER
        .ToFrozenDictionary(StringComparer.Ordinal);
#else
        ;
#endif

    /// <inheritdoc />
    /// <exception cref="JsonException">
    ///     Thrown when the JSON value is not a string, or is not one of the three status literals.
    /// </exception>
    public override TransactionInfo.TransactionStatus Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException(
                $"Expected a string value for {nameof(TransactionInfo.TransactionStatus)} but found " +
                $"{reader.TokenType}.");
        }

        // GetString() cannot return null once the token is known to be a string.
        var value = reader.GetString()!;
        if (StatusByName.TryGetValue(value, out var status))
        {
            return status;
        }

        throw new JsonException(
            $"Value {UntrustedJsonValue.Describe(value)} cannot be converted to type " +
            $"{nameof(TransactionInfo.TransactionStatus)}.");
    }

    /// <inheritdoc />
    /// <exception cref="JsonException">
    ///     Thrown when <paramref name="value" /> is not a defined <see cref="TransactionInfo.TransactionStatus" />
    ///     member, so that a bad cast cannot put a bare number on the wire in place of a status literal.
    /// </exception>
    public override void Write(Utf8JsonWriter writer, TransactionInfo.TransactionStatus value,
        JsonSerializerOptions options)
    {
        // Checking the name against the read table avoids the boxing Enum.IsDefined(Type, object) overload and
        // keeps Read and Write agreeing on the exact literal set.
        var name = value.ToString();
        if (!StatusByName.ContainsKey(name))
        {
            throw new JsonException(
                $"Value '{name}' is not a defined {nameof(TransactionInfo.TransactionStatus)}.");
        }

        writer.WriteStringValue(name);
    }
}
