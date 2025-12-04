using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Requests.SorobanRpc;

public class SorobanRpcRequest<T>
{
    public SorobanRpcRequest(string id, string method, T? parameters)
    {
        Id = id;
        Method = method;
        Params = parameters;
    }

    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; private set; } = "2.0";

    [JsonPropertyName("id")]
    public string Id { get; private set; }

    [JsonPropertyName("method")]
    public string Method { get; private set; }

    [JsonPropertyName("params")]
    public T? Params { get; private set; }
}