namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Represents a generic JSON-RPC response from the Soroban RPC server,
///     wrapping the typed result along with the request ID and JSON-RPC version.
/// </summary>
/// <typeparam name="T">The type of the result payload.</typeparam>
public class SorobanRpcResponse<T> : Response
{
    /// <summary>
    ///     The JSON-RPC request identifier that was sent with the request.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    ///     The JSON-RPC protocol version (e.g., "2.0").
    /// </summary>
    public string JsonRpc { get; init; }

    /// <summary>
    ///     The typed result payload of the JSON-RPC response.
    /// </summary>
    public T Result { get; init; }
}