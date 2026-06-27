using System;
using System.Collections.Generic;
#if !NETSTANDARD2_1
using System.Globalization;
using StellarDotnetSdk.Converters;
#endif

namespace StellarDotnetSdk.Sep.Sep0009;

internal static class KycDateFormatting
{
#if NETSTANDARD2_1
    internal static void AddIfPresent(Dictionary<string, string> result, string key, string? value)
    {
        if (value is not null)
        {
            result[key] = value;
        }
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
