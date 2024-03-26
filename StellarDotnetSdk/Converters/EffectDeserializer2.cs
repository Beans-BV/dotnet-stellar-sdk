using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.Effects;

namespace StellarDotnetSdk.Converters;

public class EffectDeserializer2:JsonConverter<EffectResponse>
{
    public override bool CanConvert(Type type) => true;
    
    public override EffectResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
            
        var typeIndicator = root.GetProperty("type_i").GetInt32();

        var targetType = typeIndicator switch
        {
            0 => typeof(AccountCreatedEffectResponse),
            1 => typeof(AccountRemovedEffectResponse),
            _ => throw new JsonException("Unknown effect response type.")
        };
        return (EffectResponse?)JsonSerializer.Deserialize(root.GetRawText(), targetType, options);
    }

    public override void Write(Utf8JsonWriter writer, EffectResponse value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}