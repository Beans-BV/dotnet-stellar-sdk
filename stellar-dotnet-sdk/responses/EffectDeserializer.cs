using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using stellar_dotnet_sdk.responses.effects;

namespace stellar_dotnet_sdk.responses;

public class EffectDeserializer : JsonConverter<EffectResponse>
{
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, EffectResponse? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override EffectResponse ReadJson(JsonReader reader, Type objectType, EffectResponse? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var type = jsonObject.GetValue("type_i");
        if (type == null) throw new ArgumentException("JSON value for type_i is missing.", nameof(type));
        var response = CreateResponse(type.ToObject<int>());
        serializer.Populate(jsonObject.CreateReader(), response);
        return response;
    }

    private static EffectResponse CreateResponse(int type)
    {
        return type switch
        {
            // Account
            0 => new AccountCreatedEffectResponse(),
            1 => new AccountRemovedEffectResponse(),
            2 => new AccountCreditedEffectResponse(),
            3 => new AccountDebitedEffectResponse(),
            4 => new AccountThresholdsUpdatedEffectResponse(),
            5 => new AccountHomeDomainUpdatedEffectResponse(),
            6 => new AccountFlagsUpdatedEffectResponse(),
            7 => new AccountInflationDestinationUpdatedEffectResponse(),
            // Signer
            10 => new SignerCreatedEffectResponse(),
            11 => new SignerRemovedEffectResponse(),
            12 => new SignerUpdatedEffectResponse(),
            // Trustline
            20 => new TrustlineCreatedEffectResponse(),
            21 => new TrustlineRemovedEffectResponse(),
            22 => new TrustlineUpdatedEffectResponse(),
            23 => new TrustlineAuthorizedEffectResponse(),
            24 => new TrustlineDeauthorizedEffectResponse(),
            25 => new TrustlineAuthorizedToMaintainLiabilitiesEffectResponse(),
            26 => new TrustlineFlagsUpdatedEffectResponse(),
            // Offer
            30 => new OfferCreatedEffectResponse(),
            31 => new OfferRemovedEffectResponse(),
            32 => new OfferUpdatedEffectResponse(),
            33 => new TradeEffectResponse(),
            // Data
            40 => new DataCreatedEffectResponse(),
            41 => new DataRemovedEffectResponse(),
            42 => new DataUpdatedEffectResponse(),
            43 => new SequenceBumpedEffectResponse(),
            // Claimable Balances
            50 => new ClaimableBalanceCreatedEffectResponse(),
            51 => new ClaimableBalanceClaimantCreatedEffectResponse(),
            52 => new ClaimableBalanceClaimedEffectResponse(),
            // Sponsorship
            60 => new AccountSponsorshipCreatedEffectResponse(),
            61 => new AccountSponsorshipUpdatedEffectResponse(),
            62 => new AccountSponsorshipRemovedEffectResponse(),
            63 => new TrustlineSponsorshipCreatedEffectResponse(),
            64 => new TrustlineSponsorshipUpdatedEffectResponse(),
            65 => new TrustlineSponsorshipRemovedEffectResponse(),
            66 => new DataSponsorshipCreatedEffectResponse(),
            67 => new DataSponsorshipUpdatedEffectResponse(),
            68 => new DataSponsorshipRemovedEffectResponse(),
            69 => new ClaimableBalanceSponsorshipCreatedEffectResponse(),
            70 => new ClaimableBalanceSponsorshipUpdatedEffectResponse(),
            71 => new ClaimableBalanceSponsorshipRemovedEffectResponse(),
            72 => new SignerSponsorshipCreatedEffectResponse(),
            73 => new SignerSponsorshipUpdatedEffectResponse(),
            74 => new SignerSponsorshipRemovedEffectResponse(),
            80 => new ClaimableBalanceClawedBackEffectResponse(),
            //Liquidity Pools
            90 => new LiquidityPoolDepositedEffectResponse(),
            91 => new LiquidityPoolWithdrewEffectResponse(),
            92 => new LiquidityPoolTradeEffectResponse(),
            93 => new LiquidityPoolCreatedEffectResponse(),
            94 => new LiquidityPoolRemovedEffectResponse(),
            95 => new LiquidityPoolRevokedEffectResponse(),
            _ => throw new JsonSerializationException($"Unknown 'type_i'='{type}'")
        };
    }
}