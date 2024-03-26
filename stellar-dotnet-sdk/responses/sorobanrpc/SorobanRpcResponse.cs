using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.sorobanrpc;

[JsonObject]
public class SorobanRpcResponse<T> : Response
{
    public SorobanRpcResponse(string jsonRpc, string id, T result)
    {
        JsonRpc = jsonRpc;
        Id = id;
        Result = result;
    }

    [JsonProperty(PropertyName = "id")] public string Id { get; }

    [JsonProperty(PropertyName = "jsonrpc")]
    public string JsonRpc { get; }

    [JsonProperty(PropertyName = "result")]
    public T Result { get; }
}