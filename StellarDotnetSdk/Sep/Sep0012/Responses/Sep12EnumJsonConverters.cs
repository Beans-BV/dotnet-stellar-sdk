using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Sep.Sep0012.Responses;

/// <summary>
///     Maps a SEP-0012 enum to and from the exact set of wire literals the specification defines.
/// </summary>
/// <remarks>
///     <para>
///         <see cref="JsonOptions.DefaultOptions" /> ends its converter list with <see cref="JsonStringEnumConverter" />,
///         which is case-insensitive, accepts bare integers by ordinal, and — lacking the underscore — cannot read
///         <c>NEEDS_INFO</c> or <c>VERIFICATION_REQUIRED</c> into PascalCase members at all. On a KYC status that
///         matters: <c>"status": 0</c> would read as <see cref="CustomerStatus.Accepted" />. The converters are
///         therefore registered on <see cref="JsonOptions.DefaultOptions" /> ahead of that catch-all, attached to each
///         enum type (for options that have no enum converter of their own), and attached to each response property —
///         the only form that outranks a converter registered on a caller's own options.
///     </para>
///     <para>
///         Matching is ordinal and case-sensitive, for values and for dictionary keys alike. A value outside the
///         defined set fails the whole response with a <see cref="JsonException" /> whose message quotes the offending
///         value in escaped, clamped form, so a hostile server cannot inflate or forge log lines with it.
///     </para>
///     <para>
///         The converters are public, like the SDK's other wire-literal converters, so that a consumer's
///         source-generated <see cref="JsonSerializerContext" /> can reference them. Subclassing is reserved to the
///         SDK.
///     </para>
/// </remarks>
/// <typeparam name="TEnum">The enum type.</typeparam>
public abstract class Sep12EnumJsonConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
{
    private readonly Dictionary<string, TEnum> _byLiteral;
    private readonly Dictionary<TEnum, string> _byValue;

    private protected Sep12EnumJsonConverter(params (string Literal, TEnum Value)[] literals)
    {
        _byLiteral = new Dictionary<string, TEnum>(StringComparer.Ordinal);
        _byValue = new Dictionary<TEnum, string>();
        foreach (var (literal, value) in literals)
        {
            _byLiteral.Add(literal, value);
            _byValue.Add(value, literal);
        }
    }

    /// <inheritdoc />
    /// <exception cref="JsonException">
    ///     Thrown when the JSON value is not a string, or is not one of the literals SEP-0012 defines.
    /// </exception>
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException(
                $"Expected a string value for {typeof(TEnum).Name} but found {reader.TokenType}.");
        }

        var value = reader.GetString();
        if (value != null && _byLiteral.TryGetValue(value, out var result))
        {
            return result;
        }

        throw new JsonException(
            $"Value {UntrustedJsonValue.Describe(value)} cannot be converted to type {typeof(TEnum).Name}.");
    }

    /// <inheritdoc />
    /// <exception cref="JsonException">Thrown when <paramref name="value" /> is not a defined member.</exception>
    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(GetLiteral(value));
    }

    /// <inheritdoc />
    /// <exception cref="JsonException">
    ///     Thrown when the property name is not one of the literals SEP-0012 defines.
    /// </exception>
    public override TEnum ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value != null && _byLiteral.TryGetValue(value, out var result))
        {
            return result;
        }

        throw new JsonException(
            $"Property name {UntrustedJsonValue.Describe(value)} cannot be converted to type {typeof(TEnum).Name}.");
    }

    /// <inheritdoc />
    /// <exception cref="JsonException">Thrown when <paramref name="value" /> is not a defined member.</exception>
    public override void WriteAsPropertyName(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(GetLiteral(value));
    }

    private string GetLiteral(TEnum value)
    {
        if (!_byValue.TryGetValue(value, out var literal))
        {
            throw new JsonException($"Value '{value}' is not a defined {typeof(TEnum).Name}.");
        }

        return literal;
    }
}

/// <summary>Wire-literal converter for <see cref="CustomerStatus" />.</summary>
public sealed class CustomerStatusJsonConverter : Sep12EnumJsonConverter<CustomerStatus>
{
    /// <summary>Initializes a new instance of the <see cref="CustomerStatusJsonConverter" /> class.</summary>
    public CustomerStatusJsonConverter()
        : base(
            ("ACCEPTED", CustomerStatus.Accepted),
            ("PROCESSING", CustomerStatus.Processing),
            ("NEEDS_INFO", CustomerStatus.NeedsInfo),
            ("REJECTED", CustomerStatus.Rejected))
    {
    }
}

/// <summary>Wire-literal converter for <see cref="ProvidedFieldStatus" />.</summary>
public sealed class ProvidedFieldStatusJsonConverter : Sep12EnumJsonConverter<ProvidedFieldStatus>
{
    /// <summary>Initializes a new instance of the <see cref="ProvidedFieldStatusJsonConverter" /> class.</summary>
    public ProvidedFieldStatusJsonConverter()
        : base(
            ("ACCEPTED", ProvidedFieldStatus.Accepted),
            ("PROCESSING", ProvidedFieldStatus.Processing),
            ("REJECTED", ProvidedFieldStatus.Rejected),
            ("VERIFICATION_REQUIRED", ProvidedFieldStatus.VerificationRequired))
    {
    }
}

/// <summary>Wire-literal converter for <see cref="FieldType" />.</summary>
public sealed class FieldTypeJsonConverter : Sep12EnumJsonConverter<FieldType>
{
    /// <summary>Initializes a new instance of the <see cref="FieldTypeJsonConverter" /> class.</summary>
    public FieldTypeJsonConverter()
        : base(
            ("string", FieldType.String),
            ("binary", FieldType.Binary),
            ("number", FieldType.Number),
            ("date", FieldType.Date))
    {
    }
}

/// <summary>
///     Reads a field's <c>choices</c> array. SEP-0012 types it only as "an array of valid values", and a field of type
///     <c>number</c> may list numbers, so a number element is kept as its JSON text (<c>[1, 2.5]</c> reads as
///     <c>"1"</c>, <c>"2.5"</c>). A <c>null</c>, boolean, object or array element fails the parse.
/// </summary>
public sealed class ChoicesJsonConverter : JsonConverter<string[]?>
{
    /// <inheritdoc />
    /// <exception cref="JsonException">
    ///     Thrown when the value is not an array of strings and numbers, or the reader ends inside the array.
    /// </exception>
    public override string[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Expected the 'choices' array but found {reader.TokenType}.");
        }

        var choices = new List<string>();
        while (true)
        {
            // JsonSerializer hands a converter the whole value, but a direct caller may pass a partial reader.
            if (!reader.Read())
            {
                throw new JsonException("The 'choices' array ends before its closing bracket.");
            }

            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    choices.Add(reader.GetString()!);
                    break;
                case JsonTokenType.Number:
                    choices.Add(Encoding.UTF8.GetString(reader.HasValueSequence
                        ? reader.ValueSequence.ToArray()
                        : reader.ValueSpan.ToArray()));
                    break;
                default:
                    throw new JsonException(
                        $"The 'choices' array contains a {reader.TokenType} element at index {choices.Count}; " +
                        "only strings and numbers are accepted.");
            }
        }

        return choices.ToArray();
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, string[]? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();
        foreach (var choice in value)
        {
            if (choice == null)
            {
                throw new JsonException("The 'choices' array contains a null element.");
            }

            writer.WriteStringValue(choice);
        }

        writer.WriteEndArray();
    }
}

/// <summary>Rejects a <c>null</c> element in the <c>files</c> array of <c>GET /customer/files</c>.</summary>
public sealed class CustomerFilesJsonConverter : NonNullElementArrayJsonConverter<CustomerFileResponse>
{
    /// <summary>Initializes a new instance of the <see cref="CustomerFilesJsonConverter" /> class.</summary>
    public CustomerFilesJsonConverter()
        : base("files")
    {
    }
}
