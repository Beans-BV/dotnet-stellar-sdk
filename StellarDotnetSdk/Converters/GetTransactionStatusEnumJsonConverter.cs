using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.SorobanRpc;

namespace StellarDotnetSdk.Converters;

public class GetTransactionStatusEnumJsonConverter : JsonConverter<GetTransactionResponse.GetTransactionStatus>
{
    public override GetTransactionResponse.GetTransactionStatus Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "NOT_FOUND" => GetTransactionResponse.GetTransactionStatus.NOT_FOUND,
            "SUCCESS" => GetTransactionResponse.GetTransactionStatus.SUCCESS,
            "FAILED" => GetTransactionResponse.GetTransactionStatus.FAILED,
            _ => throw new JsonException(
                $"Value '{value}' cannot be converted to type {nameof(GetTransactionResponse.GetTransactionStatus)}.")
        };
    }

    public override void Write(Utf8JsonWriter writer, GetTransactionResponse.GetTransactionStatus value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}