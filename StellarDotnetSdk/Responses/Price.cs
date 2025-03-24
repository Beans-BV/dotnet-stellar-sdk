using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents prices response.
/// </summary>
public class Price
{
    [JsonProperty(PropertyName = "n")] public string Numerator { get; init; }
    [JsonProperty(PropertyName = "d")] public string Denominator { get; init; }
}