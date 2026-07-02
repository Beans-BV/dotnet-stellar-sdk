using System;
using System.Collections.Generic;
using System.Globalization;
#if !NETSTANDARD2_1
using StellarDotnetSdk.Converters;
#endif

namespace StellarDotnetSdk.Sep.Sep0009;

internal static class KycDateFormatting
{
    internal const string IsoDateFormat = "yyyy-MM-dd";

#if NETSTANDARD2_1
    internal static void AddIfPresent(Dictionary<string, string> result, string key, string? value)
    {
        if (value is null)
        {
            return;
        }

        if (!DateTime.TryParseExact(value, IsoDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            throw new ArgumentException(
                $"Date value '{value}' is not in the required ISO 8601 date-only format ({IsoDateFormat}).",
                key);
        }

        result[key] = parsed.ToString(IsoDateFormat, CultureInfo.InvariantCulture);
    }
#else
    internal static void AddIfPresent(Dictionary<string, string> result, string key, DateOnly? value)
    {
        if (value.HasValue)
        {
            result[key] = value.Value.ToString(DateOnlyJsonConverter.IsoDateFormat, CultureInfo.InvariantCulture);
        }
    }
#endif
}
