using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Effects;

namespace StellarDotnetSdk.Converters;

public static class JsonOptions
{
    public static JsonSerializerOptions DefaultOptions { get; } = new()
    {
        // This allows to deserialize a number property from either a numeric string or a number 
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        WriteIndented = true,
        // This allows to deserialize the json even if the properties are not in the same case, meaning we don't have to specify the JsonPropertyName attribute for every property
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new OperationResponseJsonConverter(),
            new AssetJsonConverter(),
            new KeyPairJsonConverter(),
            new LinkJsonConverter<EffectResponse>(),
            new LinkJsonConverter<Response>(),
            new JsonStringEnumConverter(),
        },
    };
}