using System;
using System.Text;
using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses;
#nullable disable
public class AccountDataResponse : Response
{
    [JsonProperty(PropertyName = "value")] public string Value { get; init; }

    public string ValueDecoded
    {
        get
        {
            var data = Convert.FromBase64String(Value);
            return Encoding.UTF8.GetString(data);
        }
    }
}