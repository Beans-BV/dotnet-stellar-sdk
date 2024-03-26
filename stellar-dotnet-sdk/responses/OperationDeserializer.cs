using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using stellar_dotnet_sdk.responses.operations;

namespace stellar_dotnet_sdk.responses;

public class OperationDeserializer : JsonConverter<OperationResponse>
{
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, OperationResponse? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override OperationResponse ReadJson(JsonReader reader, Type objectType, OperationResponse? existingValue,
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

    private static OperationResponse CreateResponse(int type)
    {
        return type switch
        {
            0 => new CreateAccountOperationResponse(),
            1 => new PaymentOperationResponse(),
            2 => new PathPaymentStrictReceiveOperationResponse(),
            3 => new ManageSellOfferOperationResponse(),
            4 => new CreatePassiveOfferOperationResponse(),
            5 => new SetOptionsOperationResponse(),
            6 => new ChangeTrustOperationResponse(),
            7 => new AllowTrustOperationResponse(),
            8 => new AccountMergeOperationResponse(),
            9 => new InflationOperationResponse(),
            10 => new ManageDataOperationResponse(),
            11 => new BumpSequenceOperationResponse(),
            12 => new ManageBuyOfferOperationResponse(),
            13 => new PathPaymentStrictSendOperationResponse(),
            14 => new CreateClaimableBalanceOperationResponse(),
            15 => new ClaimClaimableBalanceOperationResponse(),
            16 => new BeginSponsoringFutureReservesOperationResponse(),
            17 => new EndSponsoringFutureReservesOperationResponse(),
            18 => new RevokeSponsorshipOperationResponse(),
            19 => new ClawbackOperationResponse(),
            20 => new ClawbackClaimableBalanceOperationResponse(),
            21 => new SetTrustlineFlagsOperationResponse(),
            22 => new LiquidityPoolDepositOperationResponse(),
            23 => new LiquidityPoolWithdrawOperationResponse(),
            24 => new InvokeHostFunctionOperationResponse(),
            25 => new ExtendFootprintOperationResponse(),
            26 => new RestoreFootprintOperationResponse(),
            _ => throw new JsonSerializationException($"Invalid operation 'type_i'='{type}'")
        };
    }
}