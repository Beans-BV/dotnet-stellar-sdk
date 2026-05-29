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
    ///     Converts a parsed <see cref="RetryConditionHeaderValue" /> to a positive <see cref="TimeSpan" />,
    ///     or null if the header is null, in the past, or non-positive.
    /// </summary>
    public static TimeSpan? ToTimeSpan(RetryConditionHeaderValue? header)
    {
        if (header is null)
        {
            return null;
        }

        if (header.Delta is { } delta)
        {
            return delta > TimeSpan.Zero ? delta : null;
        }

        if (header.Date is { } date)
        {
            var delay = date - DateTimeOffset.UtcNow;
            return delay > TimeSpan.Zero ? delay : null;
        }

        return null;
    }

    /// <summary>
    ///     Parses an arbitrary value (string with seconds, int, long, TimeSpan, or
    ///     <see cref="RetryConditionHeaderValue" />) into a positive <see cref="TimeSpan" />,
    ///     or null if unparseable, missing, or non-positive.
    /// </summary>
    public static TimeSpan? Parse(object? raw)
    {
        return raw switch
        {
            null => null,
            TimeSpan ts => ts > TimeSpan.Zero ? ts : null,
            RetryConditionHeaderValue header => ToTimeSpan(header),
            int seconds => FromSeconds(seconds),
            long seconds => FromSeconds(seconds),
            string s when double.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var d)
                => FromSeconds(d),
            string s when DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal,
                    out var dto)
                => dto > DateTimeOffset.UtcNow ? dto - DateTimeOffset.UtcNow : null,
            _ => null,
        };
    }

    /// <summary>
    ///     Converts a number of seconds into a positive <see cref="TimeSpan" />, or null when the value is
    ///     non-positive, NaN, or too large to represent as a <see cref="TimeSpan" />.
    /// </summary>
    private static TimeSpan? FromSeconds(double seconds)
    {
        if (double.IsNaN(seconds) || seconds <= 0 || seconds > TimeSpan.MaxValue.TotalSeconds)
        {
            return null;
        }
        return TimeSpan.FromSeconds(seconds);
    }
}