using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Effects;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses
{
    public static class JsonSingleton2
    {
        public static T? GetInstance<T>(string content)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new AssetJsonConverter());
            options.Converters.Add(new KeyPairJsonConverter());
            options.Converters.Add(new LinkJsonConverter<AssetResponse>());
            options.Converters.Add(new LinkJsonConverter<Page<AssetResponse>>());
            options.Converters.Add(new LinkJsonConverter<LedgerResponse>());
            options.Converters.Add(new LinkJsonConverter<Page<LedgerResponse>>());
            return JsonSerializer.Deserialize<T>(content, options);
        }
    }
}