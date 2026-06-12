using System;
using System.Globalization;
using System.Net.Http.Headers;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Parses HTTP <c>Retry-After</c> header values (RFC 7231 §7.1.3) into <see cref="TimeSpan" />.
///     Supports both delay-seconds and HTTP-date forms.
/// </summary>
public static class RetryAfterParser
{
    /// <summary>
    ///     Longest delay this parser reports; longer values are clamped to it. Chosen as the maximum delay
    ///     <see cref="System.Threading.Tasks.Task.Delay(TimeSpan)" /> accepts (uint.MaxValue - 1 milliseconds,
    ///     about 49.7 days), so a parsed value can always be awaited directly. A server demanding a longer
    ///     wait is telling the client to come back another day, not to actually hold a timer.
    /// </summary>
    public static readonly TimeSpan MaxRepresentableDelay = TimeSpan.FromMilliseconds(uint.MaxValue - 1);

    // Accepted HTTP-date shapes: RFC 1123 / RFC 9110 ("Tue, 25 Dec 2030 00:00:00 GMT") and ISO 8601 with a
    // time part ("2030-12-25T00:00:00Z", fractional seconds optional), which proxies/CDNs emit in practice.
    // Parsing is deliberately TryParseExact: a lenient DateTimeOffset.TryParse fallback used to read
    // malformed numerics ("12.25", "12-25", "Dec 25", "13:45") as month.day dates or times of day,
    // yielding delays of months — or a result that flipped with the wall clock.
    private static readonly string[] HttpDateFormats =
    {
        "r",
        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK",
    };

    /// <summary>
    ///     Converts a parsed <see cref="RetryConditionHeaderValue" /> to a positive <see cref="TimeSpan" />
    ///     (clamped to <see cref="MaxRepresentableDelay" />), or null if the header is null, in the past,
    ///     or non-positive.
    /// </summary>
    public static TimeSpan? ToTimeSpan(RetryConditionHeaderValue? header)
    {
        if (header is null)
        {
            return null;
        }

        if (header.Delta is { } delta)
        {
            return Positive(delta);
        }

        if (header.Date is { } date)
        {
            return FromDate(date);
        }

        return null;
    }

    /// <summary>
    ///     Parses an arbitrary value (a string in delay-seconds or HTTP-date form, int, long, TimeSpan,
    ///     <see cref="DateTime" />, <see cref="DateTimeOffset" />, or <see cref="RetryConditionHeaderValue" />)
    ///     into a positive <see cref="TimeSpan" />, or null if unparseable, missing, or non-positive.
    ///     Delays longer than <see cref="MaxRepresentableDelay" /> are clamped to it. Date strings are
    ///     accepted in RFC 1123 or ISO 8601 (with a time part) form only.
    /// </summary>
    public static TimeSpan? Parse(object? raw)
    {
        return raw switch
        {
            null => null,
            TimeSpan ts => Positive(ts),
            RetryConditionHeaderValue header => ToTimeSpan(header),
            DateTimeOffset dto => FromDate(dto),
            // An Unspecified-kind DateTime is treated as UTC, mirroring DateTimeStyles.AssumeUniversal on
            // the string path below; Local and Utc kinds convert to the exact instant they denote.
            DateTime dt => FromDate(dt.Kind == DateTimeKind.Unspecified
                ? new DateTimeOffset(dt, TimeSpan.Zero)
                : new DateTimeOffset(dt)),
            int seconds => FromSeconds(seconds),
            long seconds => FromSeconds(seconds),
            // RFC 7231 delay-seconds is a non-negative integer; parse strictly as an integer so fractional or
            // thousands-separated strings are not misread, and to stay consistent with the int/long forms and
            // the int-based RetryAfter on TooManyRequestsException / ServiceUnavailableException.
            // NumberStyles.Integer tolerates surrounding whitespace and a leading sign — deliberate leniency
            // for real-world headers (signed values come out non-positive and yield null anyway).
            string s when long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds)
                => FromSeconds(seconds),
            string s when DateTimeOffset.TryParseExact(s, HttpDateFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces, out var dto)
                => FromDate(dto),
            _ => null,
        };
    }

    /// <summary>
    ///     Converts a number of seconds into a positive <see cref="TimeSpan" /> (clamped to
    ///     <see cref="MaxRepresentableDelay" />), or null when the value is non-positive or NaN.
    /// </summary>
    private static TimeSpan? FromSeconds(double seconds)
    {
        if (double.IsNaN(seconds) || seconds <= 0)
        {
            return null;
        }

        return seconds >= MaxRepresentableDelay.TotalSeconds
            ? MaxRepresentableDelay
            : TimeSpan.FromSeconds(seconds);
    }

    /// <summary>
    ///     Converts an absolute retry time into a positive <see cref="TimeSpan" /> measured from now
    ///     (clamped to <see cref="MaxRepresentableDelay" />), or null when the time is in the past.
    ///     Reads the current time once to avoid a check/use race.
    /// </summary>
    private static TimeSpan? FromDate(DateTimeOffset date)
    {
        return Positive(date - DateTimeOffset.UtcNow);
    }

    /// <summary>Null for non-positive values; values beyond <see cref="MaxRepresentableDelay" /> clamp to it.</summary>
    private static TimeSpan? Positive(TimeSpan value)
    {
        if (value <= TimeSpan.Zero)
        {
            return null;
        }

        return value > MaxRepresentableDelay ? MaxRepresentableDelay : value;
    }
}
