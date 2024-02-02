using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.sorobanrpc;

public class SorobanRpcResponse<T> : Response
{
    [JsonProperty(PropertyName = "jsonrpc")]
    public string JsonRpc { get; private set; }

    [JsonProperty(PropertyName = "id")] public string Id { get; private set; }

    [JsonProperty(PropertyName = "result")]
    public T Result { get; private set; }
}