using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.SorobanRpc;

namespace StellarDotnetSdk.Converters;

public class SendTransactionStatusEnumJsonConverter : JsonConverter<SendTransactionResponse.SendTransactionStatus>
{
    public override SendTransactionResponse.SendTransactionStatus Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "PENDING" => SendTransactionResponse.SendTransactionStatus.PENDING,
            "TRY_AGAIN_LATER" => SendTransactionResponse.SendTransactionStatus.TRY_AGAIN_LATER,
            "DUPLICATE" => SendTransactionResponse.SendTransactionStatus.DUPLICATE,
            "ERROR" => SendTransactionResponse.SendTransactionStatus.ERROR,
            _ => throw new JsonException(
                $"Value '{value}' cannot be converted to type {nameof(SendTransactionResponse.SendTransactionStatus)}.")
        };
    }

    public override void Write(Utf8JsonWriter writer, SendTransactionResponse.SendTransactionStatus value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}