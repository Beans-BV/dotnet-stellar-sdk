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
    ///     - AllowDuplicateProperties: Rejects JSON payloads that contain the same property more than once,
    ///     preventing silent data corruption from malformed responses (critical for financial data integrity).
    ///     Available on every TFM via the System.Text.Json 10.x package reference on net8.0/netstandard2.1.
    ///     Scope: the option is enforced by STJ's built-in object mapper (POCO-bound properties); converters
    ///     that hand-parse JSON (e.g. Reserve, Asset, AssetAmount) enforce the same rule themselves via
    ///     <see cref="JsonDuplicatePropertyGuard" />. The polymorphic OperationResponse/EffectResponse
    ///     converters also guard the root document themselves: re-deserializing through the object mapper
    ///     re-detects duplicates of mapped properties, but the <c>type_i</c> discriminator they hand-read
    ///     is get-only and never mapper-bound, so a duplicated <c>type_i</c> would otherwise slip through.
    ///     - RespectNullableAnnotations: Enforces C# nullability annotations during (de)serialization,
    ///     so malformed API responses that violate the SDK's nullability contract fail fast.
    ///     Registered Converters:
    ///     - Polymorphic converters: OperationResponse, EffectResponse, Predicate
    ///     - Domain type converters: Asset, AssetAmount, KeyPair, LiquidityPoolId, LiquidityPoolClaimableAssetAmount, Reserve
    ///     - Enum converters: EventFilterType and SendTransactionStatusEnum, then
    ///     JsonStringEnumConverter (standard) last. Registration order is significant — the standard converter
    ///     matches every enum, so it must come last or it shadows the specific ones. See the comment on the
    ///     collection below. (LiquidityPoolTypeEnum is an enum converter too, but it is registered up with the
    ///     domain types; its position relative to the other two does not matter, only that it precedes the
    ///     catch-all.)
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

            // Reject JSON payloads with duplicate property names to prevent silent data corruption. Malformed
            // or adversarial responses could otherwise overwrite POCO-mapped financial fields (amount, balance,
            // destination) with attacker-controlled values without any error. The System.Text.Json 10.x package
            // reference on net8.0/netstandard2.1 makes this option available on every TFM.
            // Note: STJ enforces this option only in its built-in object mapper. Converters registered below
            // that hand-parse JSON (Reserve, Asset, AssetAmount, LiquidityPoolClaimableAssetAmount, Predicate,
            // Link, OperationResponse, EffectResponse) enforce the same rule themselves via
            // JsonDuplicatePropertyGuard. The polymorphic OperationResponse/EffectResponse converters guard
            // the root explicitly because the mapper re-parse only re-detects duplicates of mapped fields —
            // the get-only type_i discriminator they hand-read is never mapper-bound.
            AllowDuplicateProperties = false,

            // Enforce C# nullability annotations so null values for non-nullable properties are rejected
            // during deserialization.
            RespectNullableAnnotations = true,

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

                // Enum converters.
                // ORDER MATTERS, and JsonStringEnumConverter must stay last of these. System.Text.Json
                // returns the FIRST converter in this collection whose CanConvert accepts the type, and
                // JsonStringEnumConverter is a factory that accepts *every* enum — so anything after it is
                // unreachable. (A type-level [JsonConverter] does not help: this collection outranks it.
                // Only a property-level [JsonConverter] outranks this collection.)
                // - EventFilterType: Stellar RPC wants its flags joined by a bare comma ("system,contract"),
                //   whereas JsonStringEnumConverter would write "System, Contract" and RPC would reject it.
                // - SendTransactionStatus: the hand-written converter accepts only the four exact literals
                //   RPC emits. JsonStringEnumConverter, which shadowed it until this ordering was fixed, is
                //   case-insensitive and — worse — accepts bare integers, so a malformed `"status": 0` was
                //   silently read as the first member (PENDING) instead of being rejected.
                new EventFilterTypeJsonConverter(),
                new SendTransactionStatusEnumJsonConverter(),
                new JsonStringEnumConverter(),
            },
        };

        // Freeze the options to prevent accidental modification of the shared singleton.
        // populateMissingResolver: true installs the default reflection-based TypeInfoResolver,
        // which matches the SDK's existing serialization behavior.
        // STJ 8.0+ provides MakeReadOnly(bool) — satisfied on every TFM (built-in STJ 10 on net10.0;
        // the System.Text.Json 10.x package on net8.0/netstandard2.1).
        options.MakeReadOnly(true);
        return options;
    }
}