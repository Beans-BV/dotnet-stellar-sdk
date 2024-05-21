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
        return JsonConvert.DeserializeObject<T>(content);
    }
}