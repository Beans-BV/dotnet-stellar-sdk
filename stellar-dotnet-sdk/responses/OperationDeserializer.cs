﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using stellar_dotnet_sdk.responses.operations;
using System;

namespace stellar_dotnet_sdk.responses
{
    public class OperationDeserializer : JsonConverter<OperationResponse>
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, OperationResponse value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override OperationResponse ReadJson(JsonReader reader, Type objectType, OperationResponse existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var type = jsonObject.GetValue("type_i").ToObject<int>();
            var response = CreateResponse(type);
            serializer.Populate(jsonObject.CreateReader(), response);
            return response;
        }

        private static OperationResponse CreateResponse(int type)
        {
            switch (type)
            {
                case 0:
                    return new CreateAccountOperationResponse();
                case 1:
                    return new PaymentOperationResponse();
                case 2:
                    return new PathPaymentStrictReceiveOperationResponse();
                case 3:
                    return new ManageSellOfferOperationResponse();
                case 4:
                    return new CreatePassiveOfferOperationResponse();
                case 5:
                    return new SetOptionsOperationResponse();
                case 6:
                    return new ChangeTrustOperationResponse();
                case 7:
                    return new AllowTrustOperationResponse();
                case 8:
                    return new AccountMergeOperationResponse();
                case 9:
                    return new InflationOperationResponse();
                case 10:
                    return new ManageDataOperationResponse();
                case 11:
                    return new BumpSequenceOperationResponse();
                case 12:
                    return new ManageBuyOfferOperationResponse();
                case 13:
                    return new PathPaymentStrictSendOperationResponse();
                case 14:
                    return new CreateClaimableBalanceOperationResponse();
                case 15:
                    return new ClaimClaimableBalanceOperationResponse();
                case 16:
                    return new BeginSponsoringFutureReservesOperationResponse();
                case 17:
                    return new EndSponsoringFutureReservesOperationResponse();
                case 18:
                    return new RevokeSponsorshipOperationResponse();
                case 19:
                    return new ClawbackOperationResponse();
                case 20:
                    return new ClawbackClaimableBalanceOperationResponse();
                case 21:
                    return new SetTrustlineFlagsOperationResponse();
                case 22:
                    return new LiquidityPoolDepositOperationResponse();
                case 23:
                    return new LiquidityPoolWithdrawOperationResponse();
                default:
                    throw new JsonSerializationException($"Invalid operation 'type_i'='{type}'");
            }
        }
    }
}
