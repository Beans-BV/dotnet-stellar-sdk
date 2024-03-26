using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.operations;

public class ExtendFootprintOperationResponse : OperationResponse
{
    [JsonProperty(PropertyName = "extend_to")]
    public int ExtendTo;
}