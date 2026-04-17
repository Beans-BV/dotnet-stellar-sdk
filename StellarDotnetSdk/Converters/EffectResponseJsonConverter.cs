using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.Effects;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     JSON converter for polymorphic EffectResponse deserialization.
///     Uses the 'type_i' discriminator field to determine the concrete effect type.
/// </summary>
/// <remarks>
///     <p>
///         This converter handles 31+ effect types with non-sequential discriminators:
///         0-7 = Account effects
///         10-12 = Signer effects
///         20-26 = Trustline effects,
///         30-33 = Offer/Trade effects
///         40-43 = Data effects
///         50-52 = Claimable balance,
///         60-74 = Sponsorship effects
///         80 = Clawback
///         90-95 = Liquidity pool effects
///         96-97 = Soroban SAC contract transfer effects.
///         Performance: Parses JSON once into JsonDocument, then deserializes from JsonElement
///         to avoid double-parsing overhead. Dispatches via a <see cref="FrozenDictionary{TKey,TValue}" />
///         for an O(1), read-optimized lookup over the immutable discriminator set.
///         <br />
///     </p>
/// </remarks>
/// <remarks>
///     <p>
///         Designed for deserializing collections or properties typed as <see cref="EffectResponse" />.
///         Direct deserialization to concrete subclasses bypasses this converter intentionally.
///         <example>
///             Use with base type: <c>JsonSerializer.Deserialize&lt;EffectResponse&gt;(json)</c> - converter is invoked.
///             <br />
///             Not with subclass: <c>JsonSerializer.Deserialize&lt;AccountCreatedEffectResponse&gt;(json)</c> -
///             converter is bypassed.
///         </example>
///     </p>
/// </remarks>
public class EffectResponseJsonConverter : JsonConverter<EffectResponse>
{
    /// <summary>
    ///     Frozen lookup table mapping <c>type_i</c> discriminator values to concrete-type deserializers.
    ///     Built once at type initialization for optimal read performance across 40+ non-sequential keys.
    /// </summary>
    private static readonly FrozenDictionary<int, Func<JsonElement, JsonSerializerOptions, EffectResponse?>>
        Deserializers =
            new Dictionary<int, Func<JsonElement, JsonSerializerOptions, EffectResponse?>>
            {
                [0] = static (root, options) => root.Deserialize<AccountCreatedEffectResponse>(options),
                [1] = static (root, options) => root.Deserialize<AccountRemovedEffectResponse>(options),
                [2] = static (root, options) => root.Deserialize<AccountCreditedEffectResponse>(options),
                [3] = static (root, options) => root.Deserialize<AccountDebitedEffectResponse>(options),
                [4] = static (root, options) => root.Deserialize<AccountThresholdsUpdatedEffectResponse>(options),
                [5] = static (root, options) => root.Deserialize<AccountHomeDomainUpdatedEffectResponse>(options),
                [6] = static (root, options) => root.Deserialize<AccountFlagsUpdatedEffectResponse>(options),
                [7] = static (root, options) =>
                    root.Deserialize<AccountInflationDestinationUpdatedEffectResponse>(options),
                [10] = static (root, options) => root.Deserialize<SignerCreatedEffectResponse>(options),
                [11] = static (root, options) => root.Deserialize<SignerRemovedEffectResponse>(options),
                [12] = static (root, options) => root.Deserialize<SignerUpdatedEffectResponse>(options),
                [20] = static (root, options) => root.Deserialize<TrustlineCreatedEffectResponse>(options),
                [21] = static (root, options) => root.Deserialize<TrustlineRemovedEffectResponse>(options),
                [22] = static (root, options) => root.Deserialize<TrustlineUpdatedEffectResponse>(options),
                [23] = static (root, options) => root.Deserialize<TrustlineAuthorizedEffectResponse>(options),
                [24] = static (root, options) => root.Deserialize<TrustlineDeauthorizedEffectResponse>(options),
                [25] = static (root, options) =>
                    root.Deserialize<TrustlineAuthorizedToMaintainLiabilitiesEffectResponse>(options),
                [26] = static (root, options) => root.Deserialize<TrustlineFlagsUpdatedEffectResponse>(options),
                [30] = static (root, options) => root.Deserialize<OfferCreatedEffectResponse>(options),
                [31] = static (root, options) => root.Deserialize<OfferRemovedEffectResponse>(options),
                [32] = static (root, options) => root.Deserialize<OfferUpdatedEffectResponse>(options),
                [33] = static (root, options) => root.Deserialize<TradeEffectResponse>(options),
                [40] = static (root, options) => root.Deserialize<DataCreatedEffectResponse>(options),
                [41] = static (root, options) => root.Deserialize<DataRemovedEffectResponse>(options),
                [42] = static (root, options) => root.Deserialize<DataUpdatedEffectResponse>(options),
                [43] = static (root, options) => root.Deserialize<SequenceBumpedEffectResponse>(options),
                [50] = static (root, options) => root.Deserialize<ClaimableBalanceCreatedEffectResponse>(options),
                [51] = static (root, options) =>
                    root.Deserialize<ClaimableBalanceClaimantCreatedEffectResponse>(options),
                [52] = static (root, options) => root.Deserialize<ClaimableBalanceClaimedEffectResponse>(options),
                [60] = static (root, options) => root.Deserialize<AccountSponsorshipCreatedEffectResponse>(options),
                [61] = static (root, options) => root.Deserialize<AccountSponsorshipUpdatedEffectResponse>(options),
                [62] = static (root, options) => root.Deserialize<AccountSponsorshipRemovedEffectResponse>(options),
                [63] = static (root, options) => root.Deserialize<TrustlineSponsorshipCreatedEffectResponse>(options),
                [64] = static (root, options) => root.Deserialize<TrustlineSponsorshipUpdatedEffectResponse>(options),
                [65] = static (root, options) => root.Deserialize<TrustlineSponsorshipRemovedEffectResponse>(options),
                [66] = static (root, options) => root.Deserialize<DataSponsorshipCreatedEffectResponse>(options),
                [67] = static (root, options) => root.Deserialize<DataSponsorshipUpdatedEffectResponse>(options),
                [68] = static (root, options) => root.Deserialize<DataSponsorshipRemovedEffectResponse>(options),
                [69] = static (root, options) =>
                    root.Deserialize<ClaimableBalanceSponsorshipCreatedEffectResponse>(options),
                [70] = static (root, options) =>
                    root.Deserialize<ClaimableBalanceSponsorshipUpdatedEffectResponse>(options),
                [71] = static (root, options) =>
                    root.Deserialize<ClaimableBalanceSponsorshipRemovedEffectResponse>(options),
                [72] = static (root, options) => root.Deserialize<SignerSponsorshipCreatedEffectResponse>(options),
                [73] = static (root, options) => root.Deserialize<SignerSponsorshipUpdatedEffectResponse>(options),
                [74] = static (root, options) => root.Deserialize<SignerSponsorshipRemovedEffectResponse>(options),
                [80] = static (root, options) => root.Deserialize<ClaimableBalanceClawedBackEffectResponse>(options),
                [90] = static (root, options) => root.Deserialize<LiquidityPoolDepositedEffectResponse>(options),
                [91] = static (root, options) => root.Deserialize<LiquidityPoolWithdrewEffectResponse>(options),
                [92] = static (root, options) => root.Deserialize<LiquidityPoolTradeEffectResponse>(options),
                [93] = static (root, options) => root.Deserialize<LiquidityPoolCreatedEffectResponse>(options),
                [94] = static (root, options) => root.Deserialize<LiquidityPoolRemovedEffectResponse>(options),
                [95] = static (root, options) => root.Deserialize<LiquidityPoolRevokedEffectResponse>(options),
                [96] = static (root, options) => root.Deserialize<ContractCreditedEffectResponse>(options),
                [97] = static (root, options) => root.Deserialize<ContractDebitedEffectResponse>(options),
            }.ToFrozenDictionary();

    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        // Only handle the base type, not concrete subclasses
        return typeToConvert == typeof(EffectResponse);
    }

    /// <inheritdoc />
    public override EffectResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Parse JSON once into document
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        // Extract discriminator from parsed document
        if (!root.TryGetProperty("type_i", out var typeProperty))
        {
            throw new JsonException(
                "Property 'type_i' not found in JSON. " +
                "This property is required for determining the effect type."
            );
        }

        var type = typeProperty.GetInt32();

        // Dispatch via frozen lookup table (O(1) read-optimized lookup over the immutable discriminator set)
        if (!Deserializers.TryGetValue(type, out var deserializer))
        {
            throw new JsonException(
                $"Unknown effect type_i: {type}. " +
                $"Expected value in ranges 0-7, 10-12, 20-26, 30-33, 40-43, 50-52, 60-74, 80, 90-95, or 96-97. " +
                $"This may indicate an API version mismatch or a new effect type. Check if your SDK version supports this effect type."
            );
        }

        // Deserialize from already-parsed JsonElement (no double-parsing)
        return deserializer(root, options);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, EffectResponse value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}