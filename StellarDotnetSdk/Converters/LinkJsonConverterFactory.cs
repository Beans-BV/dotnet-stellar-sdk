using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Converters;

public class LinkJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType) return false;

        var genericType = typeToConvert.GetGenericTypeDefinition();
        return genericType == typeof(Link<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var responseType = typeToConvert.GetGenericArguments()[0];
        JsonConverter converter;

        // Create a specific LinkJsonConverter<T> based on the type of T
        if (responseType == typeof(AssetResponse))
        {
            converter = new LinkJsonConverter<AssetResponse>();
        }
        else if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Page<>))
        {
            var pageResponseType = responseType.GetGenericArguments()[0];
            if (pageResponseType == typeof(AssetResponse))
            {
                var converterType = typeof(LinkJsonConverter<>).MakeGenericType(responseType);
                converter = (JsonConverter)Activator.CreateInstance(converterType);
            }
            else
            {
                throw new NotSupportedException($"Type {responseType} is not supported.");
            }
        }
        else
        {
            throw new NotSupportedException($"Type {responseType} is not supported.");
        }

        return converter;
    }
}