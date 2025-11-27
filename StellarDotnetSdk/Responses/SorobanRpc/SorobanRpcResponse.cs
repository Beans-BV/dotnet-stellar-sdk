namespace StellarDotnetSdk.Responses.SorobanRpc;

public class SorobanRpcResponse<T> : Response
{
    public string Id { get; init; }
    public string JsonRpc { get; init; }
    public T Result { get; init; }
}