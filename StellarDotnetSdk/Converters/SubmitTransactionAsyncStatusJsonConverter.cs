using System;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     JSON converter for <see cref="SubmitTransactionAsyncResponse.TransactionStatus" /> that maps between the
///     Horizon <c>POST /transactions_async</c> string representations ("PENDING", "DUPLICATE", "TRY_AGAIN_LATER",
///     "ERROR") and the corresponding enum values.
/// </summary>
/// <remarks>
///     The sibling of <see cref="SendTransactionStatusEnumJsonConverter" />, for the same reason and with the same
///     registration requirement: it must stay ahead of <see cref="JsonStringEnumConverter" /> in
///     <see cref="JsonOptions.DefaultOptions" /> or the catch-all shadows it.
///     <para>
///         The catch-all accepts bare integers and maps them by ordinal, so <c>"tx_status": 0</c> read a malformed
///         response as <see cref="SubmitTransactionAsyncResponse.TransactionStatus.PENDING" /> — the most
///         optimistic of the four statuses, on the submission path callers rely on to know whether to retry. It is
///         also case-insensitive, and it happily produces undefined values: <c>"tx_status": 99</c> yielded the enum
///         value <c>99</c>, which matches none of the four members and so silently fails every comparison a caller
///         writes. Horizon passes the status through verbatim from stellar-core, which emits exactly these four
///         uppercase literals.
///     </para>
/// </remarks>
public class SubmitTransactionAsyncStatusJsonConverter : JsonConverter<SubmitTransactionAsyncResponse.TransactionStatus>
{
    /// <summary>
    ///     Frozen lookup table mapping the Horizon wire-format status strings to enum values.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, SubmitTransactionAsyncResponse.TransactionStatus>
        StatusByName =
            new Dictionary<string, SubmitTransactionAsyncResponse.TransactionStatus>(StringComparer.Ordinal)
            {
                ["PENDING"] = SubmitTransactionAsyncResponse.TransactionStatus.PENDING,
                ["DUPLICATE"] = SubmitTransactionAsyncResponse.TransactionStatus.DUPLICATE,
                ["TRY_AGAIN_LATER"] = SubmitTransactionAsyncResponse.TransactionStatus.TRY_AGAIN_LATER,
                ["ERROR"] = SubmitTransactionAsyncResponse.TransactionStatus.ERROR,
            }
#if NET8_0_OR_GREATER
                .ToFrozenDictionary(StringComparer.Ordinal);
#else
            ;
#endif

    /// <inheritdoc />
    /// <exception cref="JsonException">
    ///     Thrown when the JSON value is not a string, or is not one of the four status literals.
    /// </exception>
    public override SubmitTransactionAsyncResponse.TransactionStatus Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException(
                $"Expected a string value for {nameof(SubmitTransactionAsyncResponse.TransactionStatus)} but " +
                $"found {reader.TokenType}.");
        }

        var value = reader.GetString();
        if (value != null && StatusByName.TryGetValue(value, out var status))
        {
            return status;
        }

        throw new JsonException(
            $"Value {UntrustedJsonValue.Describe(value)} cannot be converted to type " +
            $"{nameof(SubmitTransactionAsyncResponse.TransactionStatus)}.");
    }

    /// <inheritdoc />
    /// <exception cref="JsonException">
    ///     Thrown when <paramref name="value" /> is not a defined
    ///     <see cref="SubmitTransactionAsyncResponse.TransactionStatus" /> member, so that a bad cast cannot put a
    ///     bare number on the wire in place of a status literal.
    /// </exception>
    public override void Write(Utf8JsonWriter writer, SubmitTransactionAsyncResponse.TransactionStatus value,
        JsonSerializerOptions options)
    {
        // Checking the name against the read table avoids the boxing Enum.IsDefined(Type, object) overload and
        // keeps Read and Write agreeing on the exact literal set, matching TransactionStatusJsonConverter.
        var name = value.ToString();
        if (!StatusByName.ContainsKey(name))
        {
            throw new JsonException(
                $"Value '{name}' is not a defined {nameof(SubmitTransactionAsyncResponse.TransactionStatus)}.");
        }

        writer.WriteStringValue(name);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Required because this converter is attached to the enum <em>type</em>. Without the two property-name
    ///     overloads System.Text.Json has no way to turn the enum into a JSON object key and throws
    ///     <see cref="NotSupportedException" /> for a <c>Dictionary&lt;TransactionStatus, T&gt;</c> — which is not
    ///     a <see cref="JsonException" />, so a caller's <c>catch (JsonException)</c> would miss it. The literal
    ///     set is the same one <see cref="Read" /> accepts, so a key round-trips exactly like a value.
    /// </remarks>
    /// <exception cref="JsonException">Thrown when the key is not one of the four status literals.</exception>
    public override SubmitTransactionAsyncResponse.TransactionStatus ReadAsPropertyName(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value != null && StatusByName.TryGetValue(value, out var status))
        {
            return status;
        }

        throw new JsonException(
            $"Value {UntrustedJsonValue.Describe(value)} cannot be converted to type " +
            $"{nameof(SubmitTransactionAsyncResponse.TransactionStatus)}.");
    }

    /// <inheritdoc />
    /// <exception cref="JsonException">
    ///     Thrown when <paramref name="value" /> is not a defined
    ///     <see cref="SubmitTransactionAsyncResponse.TransactionStatus" /> member.
    /// </exception>
    public override void WriteAsPropertyName(Utf8JsonWriter writer,
        SubmitTransactionAsyncResponse.TransactionStatus value, JsonSerializerOptions options)
    {
        var name = value.ToString();
        if (!StatusByName.ContainsKey(name))
        {
            throw new JsonException(
                $"Value '{name}' is not a defined {nameof(SubmitTransactionAsyncResponse.TransactionStatus)}.");
        }

        writer.WritePropertyName(name);
    }
}
