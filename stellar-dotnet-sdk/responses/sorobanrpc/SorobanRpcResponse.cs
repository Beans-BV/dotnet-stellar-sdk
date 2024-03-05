using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.sorobanrpc;

[JsonObject]
public class SorobanRpcResponse<T> : Response
{
    [JsonProperty(PropertyName = "id")] public readonly string Id;

    [JsonProperty(PropertyName = "jsonrpc")]
    public readonly string JsonRpc;

    [JsonProperty(PropertyName = "result")]
    public readonly T Result;

    public SorobanRpcResponse(string jsonRpc, string id, T result)
    {
        JsonRpc = jsonRpc;
        Id = id;
        Result = result;
    }
}