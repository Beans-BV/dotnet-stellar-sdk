#if !NETSTANDARD2_1
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     Serializes <see cref="DateOnly" /> as ISO 8601 date-only strings (yyyy-MM-dd), matching SEP-0009.
/// </summary>
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    internal const string IsoDateFormat = "yyyy-MM-dd";

    /// <inheritdoc />
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value is null)
        {
            throw new JsonException("Cannot convert null JSON value to an ISO 8601 date.");
        }

        if (!DateOnly.TryParseExact(value, IsoDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
        {
            // Keep this text in sync with IsoDateStringJsonConverter (netstandard2.1): consumers must observe
            // the same JsonException message on every target framework.
            throw new JsonException(
                $"Cannot convert JSON value '{value}' to an ISO 8601 date. Expected format: {IsoDateFormat}.");
        }

        return result;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(IsoDateFormat, CultureInfo.InvariantCulture));
    }
}
#endif
