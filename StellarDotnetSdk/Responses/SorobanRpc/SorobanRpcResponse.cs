namespace StellarDotnetSdk.Responses.SorobanRpc;

public class SorobanRpcResponse<T> : Response
{
    public SorobanRpcResponse(string jsonRpc, string id, T result)
    {
        JsonRpc = jsonRpc;
        Id = id;
        Result = result;
    }

    public string Id { get; }

    public string JsonRpc { get; }

    public T Result { get; }
}