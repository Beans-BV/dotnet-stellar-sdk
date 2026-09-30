using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0012.Responses;

/// <summary>
///     Reads a SEP-0012 timestamp, which the specification defines as a UTC ISO 8601 string. A value with a zone
///     designator or offset keeps it; a value without one is taken as UTC. System.Text.Json's own
///     <see cref="DateTimeOffset" /> handling would read it in the machine's local time zone, so the same payload
///     would name different instants on different machines.
/// </summary>
public sealed class UtcDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    /// <inheritdoc />
    /// <exception cref="JsonException">Thrown when the value is not an ISO 8601 date-time string.</exception>
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected an ISO 8601 date-time string but found {reader.TokenType}.");
        }

        // TryGetDateTime reports a value without a zone designator as DateTimeKind.Unspecified, and only such a
        // value; one with 'Z' or an offset is read through TryGetDateTimeOffset so its offset is preserved.
        if (reader.TryGetDateTime(out var dateTime) && dateTime.Kind == DateTimeKind.Unspecified)
        {
            return new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
        }

        if (reader.TryGetDateTimeOffset(out var value))
        {
            return value;
        }

        throw new JsonException("The value is not an ISO 8601 date-time string.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'",
            CultureInfo.InvariantCulture));
    }
}
