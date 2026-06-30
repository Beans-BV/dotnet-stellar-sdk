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
    ///     The instance is frozen via <see cref="JsonSerializerOptions.MakeReadOnly()" />
    ///     to prevent accidental modification at runtime.
    /// </summary>
    /// <remarks>
    ///     Configuration:
    ///     - NumberHandling: Allows reading numbers from strings (API compatibility)
    ///     - PropertyNameCaseInsensitive: Allows flexible property matching
    ///     - AllowDuplicateProperties (net10.0+ only): Rejects JSON payloads that contain the same property more than once,
    ///     preventing silent data corruption from malformed responses (critical for financial data integrity).
    ///     On net8.0/netstandard2.1 this option is unavailable; STJ uses its default duplicate-property behavior.
    ///     - RespectNullableAnnotations (net10.0+ only): Enforces C# nullability annotations during (de)serialization,
    ///     so malformed API responses that violate the SDK's nullability contract fail fast.
    ///     Registered Converters:
    ///     - Polymorphic converters: OperationResponse, EffectResponse, Predicate
    ///     - Domain type converters: Asset, AssetAmount, KeyPair, LiquidityPoolId, LiquidityPoolClaimableAssetAmount, Reserve
    ///     - Enum converters: LiquidityPoolTypeEnum, SendTransactionStatusEnum, JsonStringEnumConverter (standard)
    ///     - HATEOAS link converters: LinkJsonConverter for EffectResponse and Response
    /// </remarks>
    // A get-only property (not a field): 15.1.0 shipped this member as a property, and replacing it with a
    // field removes get_DefaultOptions() from the binary surface — consumers compiled against an older
    // package would throw MissingMethodException at runtime. Keep the property shape for binary compatibility.
    public static JsonSerializerOptions DefaultOptions { get; } = CreateDefaultOptions();

    private static JsonSerializerOptions CreateDefaultOptions()
    {
        var options = new JsonSerializerOptions
        {
            // Allow deserializing numbers from strings (API sometimes returns "123" instead of 123)
            NumberHandling = JsonNumberHandling.AllowReadingFromString,

            // Case-insensitive property matching
            PropertyNameCaseInsensitive = true,

            // AllowDuplicateProperties (net10.0+ only): Reject JSON payloads with duplicate property names to prevent
            // silent data corruption. Malformed or adversarial responses could otherwise overwrite financial fields
            // (amount, balance, destination) with attacker-controlled values without any error.
            // On net8.0/netstandard2.1 this STJ option is unavailable; deserialization uses the runtime default
            // (last duplicate property wins).
#if NET10_0_OR_GREATER
            AllowDuplicateProperties = false,
#endif

#if NET10_0_OR_GREATER
            // RespectNullableAnnotations (net10.0+ only): Enforce C# nullability annotations so null values for
            // non-nullable properties are rejected during deserialization.
            RespectNullableAnnotations = true,
#endif

            Converters =
            {
                // Polymorphic converters (MUST be registered globally)
                new OperationResponseJsonConverter(),
                new EffectResponseJsonConverter(),
                new PredicateJsonConverter(),

                // Domain type converters
                new AssetJsonConverter(),
                new AssetAmountJsonConverter(),
                new KeyPairJsonConverter(),
                new LiquidityPoolTypeEnumJsonConverter(),
                new LiquidityPoolIdJsonConverter(),
                new LiquidityPoolClaimableAssetAmountJsonConverter(),
                new ReserveJsonConverter(),

                // HATEOAS link converters
                new LinkJsonConverter<EffectResponse>(),
                new LinkJsonConverter<Response>(),

                // Enum converters
                new JsonStringEnumConverter(),
                new SendTransactionStatusEnumJsonConverter(),
            },
        };

        // Freeze the options to prevent accidental modification of the shared singleton.
        // populateMissingResolver: true installs the default reflection-based TypeInfoResolver,
        // which matches the SDK's existing serialization behavior.
        // STJ 8.0+ (including netstandard2.1 via package 8.0.5) provides MakeReadOnly(bool).
        options.MakeReadOnly(true);
        return options;
    }
}