using System.Text.Json;
#if !NETSTANDARD2_1
using StellarDotnetSdk.Converters;
#endif

namespace StellarDotnetSdk.Sep.Sep0009;

/// <summary>
///     JSON serializer options for SEP-0009 KYC field types.
///     Date fields use ISO 8601 date-only format (yyyy-MM-dd) on frameworks that support date-only types.
/// </summary>
public static class KycJsonOptions
{
    /// <summary>
    ///     Default JSON options for serializing and deserializing SEP-0009 KYC records.
    /// </summary>
    public static JsonSerializerOptions Default { get; } = CreateDefault();

    private static JsonSerializerOptions CreateDefault()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            RespectNullableAnnotations = true,
#if !NETSTANDARD2_1
            Converters =
            {
                new NullableDateOnlyJsonConverter(),
                new DateOnlyJsonConverter(),
            },
#endif
        };

#if NET8_0_OR_GREATER
        options.MakeReadOnly(true);
#endif

        return options;
    }
}
