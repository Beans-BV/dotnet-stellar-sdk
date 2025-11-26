using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.Effects;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     JSON converter for polymorphic EffectResponse deserialization.
///     Uses the 'type_i' discriminator field to determine the concrete effect type.
/// </summary>
/// <remarks>
///     This converter handles 31+ effect types with non-sequential discriminators:
///     0-7 = Account effects, 10-12 = Signer effects, 20-26 = Trustline effects,
///     30-33 = Offer/Trade effects, 40-43 = Data effects, 50-52 = Claimable balance,
///     60-74 = Sponsorship effects, 80 = Clawback, 90-95 = Liquidity pool effects.
///     Performance: Parses JSON once into JsonDocument, then deserializes from JsonElement
///     to avoid double-parsing overhead.
/// </remarks>
public class EffectResponseJsonConverter : JsonConverter<EffectResponse>
{
    public override bool CanConvert(Type typeToConvert)
    {
        // Only handle the base type, not concrete subclasses
        return typeToConvert == typeof(EffectResponse);
    }

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

        // Deserialize from already-parsed JsonElement (no double-parsing)
        return type switch
        {
            0 => root.Deserialize<AccountCreatedEffectResponse>(options),
            1 => root.Deserialize<AccountRemovedEffectResponse>(options),
            2 => root.Deserialize<AccountCreditedEffectResponse>(options),
            3 => root.Deserialize<AccountDebitedEffectResponse>(options),
            4 => root.Deserialize<AccountThresholdsUpdatedEffectResponse>(options),
            5 => root.Deserialize<AccountHomeDomainUpdatedEffectResponse>(options),
            6 => root.Deserialize<AccountFlagsUpdatedEffectResponse>(options),
            7 => root.Deserialize<AccountInflationDestinationUpdatedEffectResponse>(options),
            10 => root.Deserialize<SignerCreatedEffectResponse>(options),
            11 => root.Deserialize<SignerRemovedEffectResponse>(options),
            12 => root.Deserialize<SignerUpdatedEffectResponse>(options),
            20 => root.Deserialize<TrustlineCreatedEffectResponse>(options),
            21 => root.Deserialize<TrustlineRemovedEffectResponse>(options),
            22 => root.Deserialize<TrustlineUpdatedEffectResponse>(options),
            23 => root.Deserialize<TrustlineAuthorizedEffectResponse>(options),
            24 => root.Deserialize<TrustlineDeauthorizedEffectResponse>(options),
            25 => root.Deserialize<TrustlineAuthorizedToMaintainLiabilitiesEffectResponse>(options),
            26 => root.Deserialize<TrustlineFlagsUpdatedEffectResponse>(options),
            30 => root.Deserialize<OfferCreatedEffectResponse>(options),
            31 => root.Deserialize<OfferRemovedEffectResponse>(options),
            32 => root.Deserialize<OfferUpdatedEffectResponse>(options),
            33 => root.Deserialize<TradeEffectResponse>(options),
            40 => root.Deserialize<DataCreatedEffectResponse>(options),
            41 => root.Deserialize<DataRemovedEffectResponse>(options),
            42 => root.Deserialize<DataUpdatedEffectResponse>(options),
            43 => root.Deserialize<SequenceBumpedEffectResponse>(options),
            50 => root.Deserialize<ClaimableBalanceCreatedEffectResponse>(options),
            51 => root.Deserialize<ClaimableBalanceClaimantCreatedEffectResponse>(options),
            52 => root.Deserialize<ClaimableBalanceClaimedEffectResponse>(options),
            60 => root.Deserialize<AccountSponsorshipCreatedEffectResponse>(options),
            61 => root.Deserialize<AccountSponsorshipUpdatedEffectResponse>(options),
            62 => root.Deserialize<AccountSponsorshipRemovedEffectResponse>(options),
            63 => root.Deserialize<TrustlineSponsorshipCreatedEffectResponse>(options),
            64 => root.Deserialize<TrustlineSponsorshipUpdatedEffectResponse>(options),
            65 => root.Deserialize<TrustlineSponsorshipRemovedEffectResponse>(options),
            66 => root.Deserialize<DataSponsorshipCreatedEffectResponse>(options),
            67 => root.Deserialize<DataSponsorshipUpdatedEffectResponse>(options),
            68 => root.Deserialize<DataSponsorshipRemovedEffectResponse>(options),
            69 => root.Deserialize<ClaimableBalanceSponsorshipCreatedEffectResponse>(options),
            70 => root.Deserialize<ClaimableBalanceSponsorshipUpdatedEffectResponse>(options),
            71 => root.Deserialize<ClaimableBalanceSponsorshipRemovedEffectResponse>(options),
            72 => root.Deserialize<SignerSponsorshipCreatedEffectResponse>(options),
            73 => root.Deserialize<SignerSponsorshipUpdatedEffectResponse>(options),
            74 => root.Deserialize<SignerSponsorshipRemovedEffectResponse>(options),
            80 => root.Deserialize<ClaimableBalanceClawedBackEffectResponse>(options),
            90 => root.Deserialize<LiquidityPoolDepositedEffectResponse>(options),
            91 => root.Deserialize<LiquidityPoolWithdrewEffectResponse>(options),
            92 => root.Deserialize<LiquidityPoolTradeEffectResponse>(options),
            93 => root.Deserialize<LiquidityPoolCreatedEffectResponse>(options),
            94 => root.Deserialize<LiquidityPoolRemovedEffectResponse>(options),
            95 => root.Deserialize<LiquidityPoolRevokedEffectResponse>(options),
            _ => throw new JsonException(
                $"Unknown effect type_i: {type}. " +
                $"Expected value in ranges 0-7, 10-12, 20-26, 30-33, 40-43, 50-52, 60-74, 80, or 90-95. " +
                $"This may indicate an API version mismatch or a new effect type. Check if your SDK version supports this effect type."
            ),
        };
    }

    public override void Write(Utf8JsonWriter writer, EffectResponse value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}