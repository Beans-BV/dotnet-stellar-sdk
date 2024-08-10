using System.Collections.Generic;
using Newtonsoft.Json;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Responses;
#nullable disable

[JsonConverter(typeof(ReserveJsonConverter))]
public class Reserve
{
    [JsonProperty(PropertyName = "amount")]
    public string Amount { get; init; }

    [JsonProperty(PropertyName = "asset")] public Asset Asset { get; init; }
#nullable restore
    public override bool Equals(object? obj)
    {
        if (obj is not Reserve other)
        {
            return false;
        }

        return Equals(Asset, other.Asset) && Equals(Amount, other.Amount);
    }

    public override int GetHashCode()
    {
        var hashCode = 1588693772;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Amount);
        hashCode = hashCode * -1521134295 + EqualityComparer<Asset>.Default.GetHashCode(Asset);
        return hashCode;
    }
}