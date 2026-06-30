using System.Text.Json;
#if !NETSTANDARD2_1
using StellarDotnetSdk.Converters;
#endif

namespace StellarDotnetSdk.Sep.Sep0009;

/// <summary>
///     JSON serializer options for SEP-0009 KYC field types.
///     Date fields use ISO 8601 date-only format (yyyy-MM-dd) on frameworks that support date-only types.
/// </summary>
/// <remarks>
///     These options use <see cref="JsonNamingPolicy.CamelCase" /> (emitting e.g. <c>birthDate</c>), so they are
///     intended for internal/persistence serialization — NOT for the SEP-0009 wire format, which uses snake_case
///     field keys (e.g. <c>birth_date</c>). SEP-0009 submission/parsing goes through the typed field models and
///     their <c>*_FieldKey</c> constants, not these options. Deserializing a raw SEP-0009 server payload with
///     <see cref="Default" /> would silently skip every snake_case field.
/// </remarks>
public static class KycJsonOptions
{
    /// <summary>
    ///     Default JSON options for serializing and deserializing SEP-0009 KYC records for internal/persistence
    ///     use. Uses camelCase property names, so it does not match the SEP-0009 snake_case wire format.
    /// </summary>
    public static JsonSerializerOptions Default { get; } = CreateDefault();

    private static JsonSerializerOptions CreateDefault()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
#if NET10_0_OR_GREATER
            RespectNullableAnnotations = true,
#endif
#if !NETSTANDARD2_1
            Converters =
            {
                new NullableDateOnlyJsonConverter(),
                new DateOnlyJsonConverter(),
            },
#endif
        };

        options.MakeReadOnly(true);

        return options;
    }
}
