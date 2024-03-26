using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Operations;

public class ExtendFootprintOperationResponse : OperationResponse
{
    [JsonProperty(PropertyName = "extend_to")]
    public int ExtendTo;
}