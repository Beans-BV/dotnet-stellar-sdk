using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

public class ExtendFootprintOperationResponse : OperationResponse
{
    public override int TypeId => 25;

    [JsonPropertyName("extend_to")]
    public int ExtendTo { get; init; }
}