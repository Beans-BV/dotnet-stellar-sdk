using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     Reads an ISO 8601 timestamp that a specification defines as UTC, treating a value that carries no UTC
///     offset as UTC rather than as the local time of the machine running the SDK.
/// </summary>
/// <remarks>
///     <para>
///         System.Text.Json's built-in <see cref="DateTimeOffset" /> reader interprets an offset-less timestamp
///         such as <c>2021-04-30T07:42:23</c> in the process's local time zone, so the same payload yielded a
///         different instant on every machine whose zone is not UTC. SEP-38 specifies its timestamps as "UTC ISO
///         8601" and its own example of <c>expires_at</c> omits the designator, so that reading would move a firm
///         quote's expiry by the host's UTC offset. A value that does carry <c>Z</c> or an explicit offset is read
///         as that instant.
///     </para>
///     <para>
///         A value must satisfy both <see cref="Utf8JsonReader.TryGetDateTimeOffset" /> (the ISO 8601-1 extended
///         profile) and <see cref="DateTimeOffset.TryParse(string, IFormatProvider, DateTimeStyles, out DateTimeOffset)" />;
///         anything else throws <see cref="JsonException" />. More than seven fractional-second digits are rounded
///         to the nearest 100 ns tick.
///     </para>
///     <para>
///         Writes UTC with a <c>Z</c> designator and only as many fractional-second digits as are non-zero.
///     </para>
///     <para>
///         Attached per property with <see cref="JsonConverterAttribute" />. It is public so that a consumer's
///         source-generated <see cref="JsonSerializerContext" /> can instantiate it.
///     </para>
/// </remarks>
public sealed class UtcDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    /// <inheritdoc />
    /// <exception cref="JsonException">Thrown when the token is not an ISO 8601 timestamp string.</exception>
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException(
                $"Expected an ISO 8601 timestamp as a JSON string, but found a {reader.TokenType} token.");
        }

        var text = reader.GetString()!;
        if (!reader.TryGetDateTimeOffset(out _))
        {
            throw new JsonException($"The value {UntrustedJsonValue.Describe(text)} is not an ISO 8601 timestamp.");
        }

        // The re-parse only decides how a missing offset is read: AssumeUniversal applies to an offset-less value
        // alone, and an explicit offset or Z still wins. The two parsers' grammars are not identical — the reader
        // accepts a fraction separator with no digits ("07:42:23.Z") and rounds nothing, where Parse rejects the
        // first and rounds an eight-digit fraction up past 9999-12-31 — so a value only the reader accepts must
        // still fail as a JsonException, never as the FormatException Parse would throw.
        if (!DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal,
                out var value))
        {
            throw new JsonException($"The value {UntrustedJsonValue.Describe(text)} is not an ISO 8601 timestamp.");
        }

        return value;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Format(value));
    }

    /// <summary>
    ///     Formats <paramref name="value" /> as a UTC ISO 8601 timestamp with a <c>Z</c> designator.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <returns>For example <c>2021-04-30T07:42:23Z</c> or <c>2021-04-30T07:42:23.5Z</c>.</returns>
    internal static string Format(DateTimeOffset value)
    {
        // The F specifiers drop trailing zeros, and the separator with them when the fraction is zero.
        return value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'", CultureInfo.InvariantCulture);
    }
}
