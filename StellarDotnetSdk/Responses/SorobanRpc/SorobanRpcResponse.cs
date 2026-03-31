namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Represents a generic JSON-RPC response from the Soroban RPC server,
///     wrapping the typed result along with the request ID and JSON-RPC version.
/// </summary>
/// <typeparam name="T">The type of the result payload.</typeparam>
public class SorobanRpcResponse<T> : Response
{
    public string Id { get; init; }
    public string JsonRpc { get; init; }
    public T Result { get; init; }
}