using System.Text.Json;

namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Represents the <c>error</c> member of a JSON-RPC 2.0 response (JSON-RPC 2.0 §5.1) returned by the
///     Stellar RPC server. A response carrying this member has no <c>result</c> member.
/// </summary>
public class SorobanRpcErrorResponse
{
    /// <summary>
    ///     The error code indicating the error type that occurred.
    /// </summary>
    public int Code { get; init; }

    /// <summary>
    ///     A short description of the error, or null if the server omitted it.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    ///     Additional information about the error, or null if the server omitted it. The JSON-RPC
    ///     specification allows any JSON value here, so it is preserved verbatim as a <see cref="JsonElement" />.
    /// </summary>
    public JsonElement? Data { get; init; }
}
