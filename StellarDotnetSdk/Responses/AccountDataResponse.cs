using System;
using System.Text;
using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses;

public class AccountDataResponse : Response
{
    public AccountDataResponse(string value)
    {
        Value = value;
    }

    [JsonProperty(PropertyName = "value")] public string Value { get; private set; }

    public string ValueDecoded
    {
        get
        {
            var data = Convert.FromBase64String(Value);
            return Encoding.UTF8.GetString(data);
        }
    }
}