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
    ///     - PropertyNameCaseInsensitive: Allows flexible property matching
    ///     - AllowDuplicateProperties: Rejects JSON payloads that contain the same property more than once,
    ///     preventing silent data corruption from malformed responses (critical for financial data integrity).
    ///     Registered Converters:
    ///     - Polymorphic converters: OperationResponse, EffectResponse, Predicate
    ///     - Domain type converters: Asset, AssetAmount, KeyPair, LiquidityPoolId, LiquidityPoolClaimableAssetAmount, Reserve
    ///     - Enum converters: LiquidityPoolTypeEnum, SendTransactionStatusEnum, JsonStringEnumConverter (standard)
    ///     - HATEOAS link converters: LinkJsonConverter for EffectResponse and Response
    /// </remarks>
    public static JsonSerializerOptions DefaultOptions { get; } = new()
    {
        // Allow deserializing numbers from strings (API sometimes returns "123" instead of 123)
        NumberHandling = JsonNumberHandling.AllowReadingFromString,

        // Case-insensitive property matching
        PropertyNameCaseInsensitive = true,

        // Reject JSON payloads with duplicate property names to prevent silent data corruption.
        // Malformed or adversarial responses could otherwise overwrite financial fields (amount,
        // balance, destination) with attacker-controlled values without any error.
        AllowDuplicateProperties = false,

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
}