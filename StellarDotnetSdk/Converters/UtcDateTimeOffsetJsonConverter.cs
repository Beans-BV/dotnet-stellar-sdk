using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

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
///         A value must be in the ISO 8601-1 extended profile that <see cref="Utf8JsonReader.TryGetDateTimeOffset" />
///         reads — a date, optionally followed by <c>T</c>, hours and minutes, optional seconds with an optional
///         fraction of 1 to 16 digits, and an optional <c>Z</c>, <c>±hh</c> or <c>±hh:mm</c> offset — and must
///         satisfy <see cref="DateTimeOffset.TryParse(string, IFormatProvider, DateTimeStyles, out DateTimeOffset)" />;
///         anything else throws <see cref="JsonException" />. The profile is checked on the text, so the outcome does
///         not depend on the time zone of the machine running the SDK. More than seven fractional-second digits are
///         rounded to the nearest 100 ns tick.
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
    /// <summary>
    ///     The ISO 8601-1 extended profile that <see cref="Utf8JsonReader.TryGetDateTimeOffset" /> reads: a date,
    ///     optionally followed by a time and then an offset. ASCII digits only, and <c>\z</c> rather than <c>$</c>,
    ///     which would also match before a trailing line feed.
    /// </summary>
    private static readonly Regex Iso8601ExtendedProfile = new(
        @"\A[0-9]{4}-[0-9]{2}-[0-9]{2}" +
        @"(T[0-9]{2}:[0-9]{2}(:[0-9]{2}(\.[0-9]{1,16})?)?(Z|[+-][0-9]{2}(:[0-9]{2})?)?)?\z");

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
        // The grammar is checked on the text, not with reader.TryGetDateTimeOffset: that method reads an
        // offset-less value as local time, so "9999-12-31T23:59:59" failed it in a zone west of UTC and "0001-01-01"
        // in one east of it. Parse then decides the instant: AssumeUniversal applies to an offset-less value alone,
        // and an explicit offset or Z still wins. A value Parse rejects (an invalid date, an instant outside the
        // range, an eight-digit fraction that rounds past 9999-12-31) must still fail as a JsonException, never as
        // the FormatException Parse would throw.
        if (!Iso8601ExtendedProfile.IsMatch(text) ||
            !DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal,
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
