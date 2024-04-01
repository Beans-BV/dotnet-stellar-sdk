using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses;

public class Claimant
{
    public Claimant(string destination, Predicate predicate)
    {
        Destination = destination;
        Predicate = predicate;
    }

    [JsonProperty(PropertyName = "destination")]
    public string Destination { get; set; }

    [JsonProperty(PropertyName = "predicate")]
    public Predicate Predicate { get; set; }
}