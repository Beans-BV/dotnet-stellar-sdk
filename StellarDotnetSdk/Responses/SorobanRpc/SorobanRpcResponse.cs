namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Represents a generic JSON-RPC response from the Soroban RPC server,
///     wrapping the typed result along with the request ID and JSON-RPC version.
/// </summary>
/// <typeparam name="T">The type of the result payload.</typeparam>
public class SorobanRpcResponse<T> : Response
{
    /// <summary>
    ///     The JSON-RPC request identifier echoed back by the server, normally the one sent with the request.
    ///     JSON-RPC 2.0 §5 requires it to be null in exactly one case — the server could not read the request's
    ///     id at all, a parse error or an invalid request — and such a response carries <see cref="Error" />.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    ///     The JSON-RPC protocol version (e.g., "2.0").
    /// </summary>
    public string JsonRpc { get; init; }

    /// <summary>
    ///     The typed result payload of the JSON-RPC response, or null when the server answered with
    ///     <see cref="Error" /> instead — a JSON-RPC error response carries no result. It is also null for a
    ///     malformed envelope that carries neither member, which <see cref="Soroban.StellarRpcServer" />
    ///     rejects rather than passing on.
    /// </summary>
    public T? Result { get; init; }

    /// <summary>
    ///     The JSON-RPC error returned by the server, or null when the request succeeded. A JSON-RPC error
    ///     response carries no <see cref="Result" /> and normally arrives with HTTP status 200.
    /// </summary>
    public SorobanRpcErrorResponse? Error { get; init; }
}