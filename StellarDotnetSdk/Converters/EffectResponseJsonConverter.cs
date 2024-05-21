using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.Effects;

namespace StellarDotnetSdk.Converters;

public class EffectResponseJsonConverter : JsonConverter<EffectResponse>
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsAssignableFrom(typeof(EffectResponse)) && typeToConvert.IsAbstract;

    public override EffectResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        var root = jsonDocument.RootElement;
        var type = root.GetProperty("type_i").GetInt32();
        var rawText = root.GetRawText();
        return type switch
        {
            0 => JsonSerializer.Deserialize<AccountCreatedEffectResponse>(rawText, options),
            1 => JsonSerializer.Deserialize<AccountRemovedEffectResponse>(rawText, options),
            2 => JsonSerializer.Deserialize<AccountCreditedEffectResponse>(rawText, options),
            3 => JsonSerializer.Deserialize<AccountDebitedEffectResponse>(rawText, options),
            4 => JsonSerializer.Deserialize<AccountThresholdsUpdatedEffectResponse>(rawText, options),
            5 => JsonSerializer.Deserialize<AccountHomeDomainUpdatedEffectResponse>(rawText, options),
            6 => JsonSerializer.Deserialize<AccountFlagsUpdatedEffectResponse>(rawText, options),
            7 => JsonSerializer.Deserialize<AccountInflationDestinationUpdatedEffectResponse>(rawText, options),
            10 => JsonSerializer.Deserialize<SignerCreatedEffectResponse>(rawText, options),
            11 => JsonSerializer.Deserialize<SignerRemovedEffectResponse>(rawText, options),
            12 => JsonSerializer.Deserialize<SignerUpdatedEffectResponse>(rawText, options),
            20 => JsonSerializer.Deserialize<TrustlineCreatedEffectResponse>(rawText, options),
            21 => JsonSerializer.Deserialize<TrustlineRemovedEffectResponse>(rawText, options),
            22 => JsonSerializer.Deserialize<TrustlineUpdatedEffectResponse>(rawText, options),
            23 => JsonSerializer.Deserialize<TrustlineAuthorizedEffectResponse>(rawText, options),
            24 => JsonSerializer.Deserialize<TrustlineDeauthorizedEffectResponse>(rawText, options),
            25 => JsonSerializer.Deserialize<TrustlineAuthorizedToMaintainLiabilitiesEffectResponse>(rawText, options),
            26 => JsonSerializer.Deserialize<TrustlineFlagsUpdatedEffectResponse>(rawText, options),
            30 => JsonSerializer.Deserialize<OfferCreatedEffectResponse>(rawText, options),
            31 => JsonSerializer.Deserialize<OfferRemovedEffectResponse>(rawText, options),
            32 => JsonSerializer.Deserialize<OfferUpdatedEffectResponse>(rawText, options),
            33 => JsonSerializer.Deserialize<TradeEffectResponse>(rawText, options),
            40 => JsonSerializer.Deserialize<DataCreatedEffectResponse>(rawText, options),
            41 => JsonSerializer.Deserialize<DataRemovedEffectResponse>(rawText, options),
            42 => JsonSerializer.Deserialize<DataUpdatedEffectResponse>(rawText, options),
            43 => JsonSerializer.Deserialize<SequenceBumpedEffectResponse>(rawText, options),
            50 => JsonSerializer.Deserialize<ClaimableBalanceCreatedEffectResponse>(rawText, options),
            51 => JsonSerializer.Deserialize<ClaimableBalanceClaimantCreatedEffectResponse>(rawText, options),
            52 => JsonSerializer.Deserialize<ClaimableBalanceClaimedEffectResponse>(rawText, options),
            60 => JsonSerializer.Deserialize<AccountSponsorshipCreatedEffectResponse>(rawText, options),
            61 => JsonSerializer.Deserialize<AccountSponsorshipUpdatedEffectResponse>(rawText, options),
            62 => JsonSerializer.Deserialize<AccountSponsorshipRemovedEffectResponse>(rawText, options),
            63 => JsonSerializer.Deserialize<TrustlineSponsorshipCreatedEffectResponse>(rawText, options),
            64 => JsonSerializer.Deserialize<TrustlineSponsorshipUpdatedEffectResponse>(rawText, options),
            65 => JsonSerializer.Deserialize<TrustlineSponsorshipRemovedEffectResponse>(rawText, options),
            66 => JsonSerializer.Deserialize<DataSponsorshipCreatedEffectResponse>(rawText, options),
            67 => JsonSerializer.Deserialize<DataSponsorshipUpdatedEffectResponse>(rawText, options),
            68 => JsonSerializer.Deserialize<DataSponsorshipRemovedEffectResponse>(rawText, options),
            69 => JsonSerializer.Deserialize<ClaimableBalanceSponsorshipCreatedEffectResponse>(rawText, options),
            70 => JsonSerializer.Deserialize<ClaimableBalanceSponsorshipUpdatedEffectResponse>(rawText, options),
            71 => JsonSerializer.Deserialize<ClaimableBalanceSponsorshipRemovedEffectResponse>(rawText, options),
            72 => JsonSerializer.Deserialize<SignerSponsorshipCreatedEffectResponse>(rawText, options),
            73 => JsonSerializer.Deserialize<SignerSponsorshipUpdatedEffectResponse>(rawText, options),
            74 => JsonSerializer.Deserialize<SignerSponsorshipRemovedEffectResponse>(rawText, options),
            80 => JsonSerializer.Deserialize<ClaimableBalanceClawedBackEffectResponse>(rawText, options),
            90 => JsonSerializer.Deserialize<LiquidityPoolDepositedEffectResponse>(rawText, options),
            91 => JsonSerializer.Deserialize<LiquidityPoolWithdrewEffectResponse>(rawText, options),
            92 => JsonSerializer.Deserialize<LiquidityPoolTradeEffectResponse>(rawText, options),
            93 => JsonSerializer.Deserialize<LiquidityPoolCreatedEffectResponse>(rawText, options),
            94 => JsonSerializer.Deserialize<LiquidityPoolRemovedEffectResponse>(rawText, options),
            95 => JsonSerializer.Deserialize<LiquidityPoolRevokedEffectResponse>(rawText, options),
            _ => throw new JsonException("Unknown type.")
        };
    }

    public override void Write(Utf8JsonWriter writer, EffectResponse value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}