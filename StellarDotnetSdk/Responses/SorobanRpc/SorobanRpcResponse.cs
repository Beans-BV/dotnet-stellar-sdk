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
    ///     The typed result payload of the JSON-RPC response. Absent — and therefore null — when the server
    ///     answered with <see cref="Error" /> instead.
    /// </summary>
    public T Result { get; init; }

    /// <summary>
    ///     The JSON-RPC error returned by the server, or null when the request succeeded. A JSON-RPC error
    ///     response carries no <see cref="Result" /> and normally arrives with HTTP status 200.
    /// </summary>
    public SorobanRpcErrorResponse? Error { get; init; }
}