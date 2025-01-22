using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.Operations;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace StellarDotnetSdk.Converters;

public class OperationResponseJsonConverter : JsonConverter<OperationResponse>
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsAssignableFrom(typeof(OperationResponse)) && typeToConvert.IsAbstract;
    
    public override OperationResponse? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        var type = root.GetProperty("type_i").GetInt32();
        var rawText = root.GetRawText();
        return type switch
        {
            0 => JsonSerializer.Deserialize<CreateAccountOperationResponse>(rawText, options),
            1 => JsonSerializer.Deserialize<PaymentOperationResponse>(rawText, options),
            2 => JsonSerializer.Deserialize<PathPaymentStrictReceiveOperationResponse>(rawText, options),
            3 => JsonSerializer.Deserialize<ManageSellOfferOperationResponse>(rawText, options),
            4 => JsonSerializer.Deserialize<CreatePassiveOfferOperationResponse>(rawText, options),
            5 => JsonSerializer.Deserialize<SetOptionsOperationResponse>(rawText, options),
            6 => JsonSerializer.Deserialize<ChangeTrustOperationResponse>(rawText, options),
            7 => JsonSerializer.Deserialize<AllowTrustOperationResponse>(rawText, options),
            8 => JsonSerializer.Deserialize<AccountMergeOperationResponse>(rawText, options),
            9 => JsonSerializer.Deserialize<InflationOperationResponse>(rawText, options),
            10 => JsonSerializer.Deserialize<ManageDataOperationResponse>(rawText, options),
            11 => JsonSerializer.Deserialize<BumpSequenceOperationResponse>(rawText, options),
            12 => JsonSerializer.Deserialize<ManageBuyOfferOperationResponse>(rawText, options),
            13 => JsonSerializer.Deserialize<PathPaymentStrictSendOperationResponse>(rawText, options),
            14 => JsonSerializer.Deserialize<CreateClaimableBalanceOperationResponse>(rawText, options),
            15 => JsonSerializer.Deserialize<ClaimClaimableBalanceOperationResponse>(rawText, options),
            16 => JsonSerializer.Deserialize<BeginSponsoringFutureReservesOperationResponse>(rawText, options),
            17 => JsonSerializer.Deserialize<EndSponsoringFutureReservesOperationResponse>(rawText, options),
            18 => JsonSerializer.Deserialize<RevokeSponsorshipOperationResponse>(rawText, options),
            19 => JsonSerializer.Deserialize<ClawbackOperationResponse>(rawText, options),
            20 => JsonSerializer.Deserialize<ClawbackClaimableBalanceOperationResponse>(rawText, options),
            21 => JsonSerializer.Deserialize<SetTrustlineFlagsOperationResponse>(rawText, options),
            22 => JsonSerializer.Deserialize<LiquidityPoolDepositOperationResponse>(rawText, options),
            23 => JsonSerializer.Deserialize<LiquidityPoolWithdrawOperationResponse>(rawText, options),
            24 => JsonSerializer.Deserialize<InvokeHostFunctionOperationResponse>(rawText, options),
            25 => JsonSerializer.Deserialize<ExtendFootprintOperationResponse>(rawText, options),
            26 => JsonSerializer.Deserialize<RestoreFootprintOperationResponse>(rawText, options),
            _ => throw new JsonException("Unknown type.")
        };
    }
    
    public override void Write(Utf8JsonWriter writer, OperationResponse value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}