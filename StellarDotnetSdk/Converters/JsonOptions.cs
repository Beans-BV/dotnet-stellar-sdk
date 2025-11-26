using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Effects;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     Centralized JSON serializer options used throughout the SDK.
///     All API responses use these options for consistent behavior.
/// </summary>
public static class JsonOptions
{
    /// <summary>
    ///     Default JSON serializer options with all custom converters registered.
    /// </summary>
    /// <remarks>
    ///     Configuration:
    ///     - NumberHandling: Allows reading numbers from strings (API compatibility)
    ///     - WriteIndented: Pretty-prints JSON for debugging
    ///     - PropertyNameCaseInsensitive: Allows flexible property matching
    ///     Registered Converters:
    ///     - Polymorphic converters for operations and effects
    ///     - Domain type converters for assets, key pairs, etc.
    ///     - HATEOAS link converters
    ///     - Standard enum converter
    /// </remarks>
    public static JsonSerializerOptions DefaultOptions { get; } = new()
    {
        // Allow deserializing numbers from strings (API sometimes returns "123" instead of 123)
        NumberHandling = JsonNumberHandling.AllowReadingFromString,

        // Pretty-print for better debugging
        WriteIndented = true,

        // Case-insensitive property matching
        PropertyNameCaseInsensitive = true,

        Converters =
        {
            // Polymorphic converters (MUST be registered globally)
            new OperationResponseJsonConverter(),
            new EffectResponseJsonConverter(),

            // Domain type converters
            new AssetJsonConverter(),
            new KeyPairJsonConverter(),
            new LiquidityPoolTypeEnumJsonConverter(),

            // HATEOAS link converters
            new LinkJsonConverter<EffectResponse>(),
            new LinkJsonConverter<Response>(),

            // Standard enum converter
            new JsonStringEnumConverter(),
        },
    };
}