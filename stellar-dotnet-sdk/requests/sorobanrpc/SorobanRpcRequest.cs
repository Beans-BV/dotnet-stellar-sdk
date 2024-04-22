using Newtonsoft.Json;

namespace stellar_dotnet_sdk.requests.sorobanrpc;

public class SorobanRpcRequest<T>
{
    public SorobanRpcRequest(string id, string method, T? parameters)
    {
        Id = id;
        Method = method;
        Params = parameters;
    }

    [JsonProperty(PropertyName = "jsonrpc")]
    public string JsonRpc { get; private set; } = "2.0";

    [JsonProperty(PropertyName = "id")] public string Id { get; private set; }

    [JsonProperty(PropertyName = "method")]
    public string Method { get; private set; }

    [JsonProperty(PropertyName = "params")]
    public T? Params { get; private set; }
}