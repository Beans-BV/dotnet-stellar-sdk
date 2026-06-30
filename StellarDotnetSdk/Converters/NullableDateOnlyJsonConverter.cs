#if !NETSTANDARD2_1
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     Serializes nullable <see cref="DateOnly" /> as ISO 8601 date-only strings (yyyy-MM-dd), matching SEP-0009.
/// </summary>
public class NullableDateOnlyJsonConverter : JsonConverter<DateOnly?>
{
    /// <inheritdoc />
    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

        if (!DateOnly.TryParseExact(value, DateOnlyJsonConverter.IsoDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
        {
            throw new JsonException(
                $"Cannot convert JSON value '{value}' to DateOnly. Expected format: {DateOnlyJsonConverter.IsoDateFormat}.");
        }

        return result;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value.ToString(DateOnlyJsonConverter.IsoDateFormat, CultureInfo.InvariantCulture));
    }
}
#endif
