using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class Claimant
{
    [JsonProperty(PropertyName = "destination")]
    public string Destination { get; init; }

    [JsonProperty(PropertyName = "predicate")]
    public Predicate Predicate { get; init; }
}