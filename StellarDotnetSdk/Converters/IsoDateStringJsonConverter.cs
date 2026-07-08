#if NETSTANDARD2_1
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     Validates string-typed date properties as ISO 8601 date-only strings (yyyy-MM-dd), matching SEP-0009.
///     This is the netstandard2.1 counterpart of <c>NullableDateOnlyJsonConverter</c>: on frameworks without
///     <c>DateOnly</c>, date fields are plain strings, and this converter rejects any value that is not a valid
///     yyyy-MM-dd date on both read and write so all target frameworks accept and emit identical JSON.
///     Kept <c>internal</c>: it is applied via <c>[JsonConverter(typeof(...))]</c> within this assembly and
///     must not add a public type to the netstandard2.1 surface that the net8.0/net10.0 builds lack
///     (cross-TFM package-validation, CP0001).
/// </summary>
internal sealed class IsoDateStringJsonConverter : JsonConverter<string?>
{
    internal const string IsoDateFormat = "yyyy-MM-dd";

    /// <inheritdoc />
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        var value = reader.GetString();
        if (value is null)
        {
            return null;
        }

        Validate(value);
        return value;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        Validate(value);
        writer.WriteStringValue(value);
    }

    private static void Validate(string value)
    {
        if (!DateTime.TryParseExact(value, IsoDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            // Keep this text in sync with DateOnlyJsonConverter/NullableDateOnlyJsonConverter (net8.0/net10.0):
            // consumers must observe the same JsonException message on every target framework.
            throw new JsonException(
                $"Cannot convert JSON value '{value}' to an ISO 8601 date. Expected format: {IsoDateFormat}.");
        }
    }
}
#endif