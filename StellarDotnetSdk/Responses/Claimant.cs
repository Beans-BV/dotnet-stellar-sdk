using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class Claimant
{
    [JsonPropertyName("destination")]
    public string Destination { get; init; }

    [JsonPropertyName("predicate")]
    public Predicate Predicate { get; init; }
}