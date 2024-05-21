using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents prices response.
/// </summary>
public class Price
{
    [JsonPropertyName("n")]
    public string Numerator { get; init; }

    [JsonPropertyName("d")]
    public string Denominator { get; init; }
}