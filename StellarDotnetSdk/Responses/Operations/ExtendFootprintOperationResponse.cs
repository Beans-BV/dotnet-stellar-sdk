using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Operations;

public class ExtendFootprintOperationResponse : OperationResponse
{
    public override int TypeId => 25;

    [JsonProperty(PropertyName = "extend_to")]
    public int ExtendTo { get; init; }
}