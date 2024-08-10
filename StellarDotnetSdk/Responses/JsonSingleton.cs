using System.Linq;
using Newtonsoft.Json;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Effects;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses;

public static class JsonSingleton
{
    public static T? GetInstance<T>(string content)
    {
        var pageResponseConversions = new[]
        {
            typeof(Page<AccountResponse>),
            typeof(Page<AssetResponse>),
            typeof(Page<EffectResponse>),
            typeof(Page<LedgerResponse>),
            typeof(Page<OfferResponse>),
            typeof(Page<OperationResponse>),
            typeof(Page<PathResponse>),
            typeof(Page<TransactionResponse>),
            typeof(Page<TradeResponse>),
            typeof(Page<TradeAggregationResponse>),
            typeof(Page<TransactionResponse>),
        };

        var jsonConverters = new JsonConverter[]
        {
            new AssetJsonConverter(),
            new KeyPairJsonConverter(),
            new OperationResponseJsonConverter(),
            new EffectResponseJsonConverter(),
        };

        var pageJsonConverters = new JsonConverter[]
        {
            new AssetJsonConverter(),
            new KeyPairJsonConverter(),
            new OperationResponseJsonConverter(),
            new EffectResponseJsonConverter(),
        };

        if (pageResponseConversions.Contains(typeof(T)))
        {
            return JsonConvert.DeserializeObject<T>(content, pageJsonConverters);
        }

        return JsonConvert.DeserializeObject<T>(content, jsonConverters);
    }
}