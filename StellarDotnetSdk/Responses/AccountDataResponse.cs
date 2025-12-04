using System;
using System.Text;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;
#nullable disable
public class AccountDataResponse : Response
{
    [JsonPropertyName("value")]
    public string Value { get; init; }

    public string ValueDecoded
    {
        get
        {
            var data = Convert.FromBase64String(Value);
            return Encoding.UTF8.GetString(data);
        }
    }
}