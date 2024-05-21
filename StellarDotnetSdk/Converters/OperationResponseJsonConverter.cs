using System;
using System.Text.Json;
using StellarDotnetSdk.Responses.Operations;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace StellarDotnetSdk.Converters;

public class OperationResponseJsonConverter : System.Text.Json.Serialization.JsonConverter<OperationResponse>
{
    public override void Write(Utf8JsonWriter writer, OperationResponse value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
    
    public override OperationResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        string type = root.GetProperty("Type").GetString();

        switch (type)
        {
            case "0":
                return JsonSerializer.Deserialize<CreateAccountOperationResponse>(root.GetRawText(), options);
            case "1":
                return JsonSerializer.Deserialize<PaymentOperationResponse>(root.GetRawText(), options);
            default:
                throw new JsonException("Unknown type.");
        }
    }

    // public override OperationResponse ReadJson(JsonReader reader, Type objectType, OperationResponse? existingValue,
    //     bool hasExistingValue,
    //     JsonSerializer serializer)
    // {
    //     var jsonObject = JObject.Load(reader);
    //     var type = jsonObject.GetValue("type_i");
    //     if (type == null) throw new ArgumentException("JSON value for type_i is missing.", nameof(type));
    //     var response = CreateResponse(type.ToObject<int>());
    //     serializer. Populate(jsonObject.CreateReader(), response);
    //     return response;
    // }

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
            _ => throw new JsonException($"Invalid operation 'type_i'='{type}'")
        };
    }
}