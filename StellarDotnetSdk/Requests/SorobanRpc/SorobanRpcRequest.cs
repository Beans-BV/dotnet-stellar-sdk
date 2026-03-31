using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Requests.SorobanRpc;

/// <summary>
///     Represents a JSON-RPC 2.0 request envelope for Soroban RPC calls.
/// </summary>
/// <typeparam name="T">The type of the request parameters payload.</typeparam>
public class SorobanRpcRequest<T>
{
    /// <summary>
    ///     Initializes a new JSON-RPC 2.0 request for a Soroban RPC call.
    /// </summary>
    /// <param name="id">The unique request identifier.</param>
    /// <param name="method">The JSON-RPC method name (e.g. <c>getEvents</c>, <c>getLedgerEntries</c>).</param>
    /// <param name="parameters">The method-specific parameters, or null if none.</param>
    public SorobanRpcRequest(string id, string method, T? parameters)
    {
        Id = id;
        Method = method;
        Params = parameters;
    }

    /// <summary>
    ///     The JSON-RPC protocol version. Always <c>"2.0"</c>.
    /// </summary>
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; private set; } = "2.0";

    /// <summary>
    ///     The unique identifier for this JSON-RPC request.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; private set; }

    /// <summary>
    ///     The JSON-RPC method name to invoke on the Soroban RPC server.
    /// </summary>
    [JsonPropertyName("method")]
    public string Method { get; private set; }

    /// <summary>
    ///     The method-specific parameters payload, or null if the method takes no parameters.
    /// </summary>
    [JsonPropertyName("params")]
    public T? Params { get; private set; }
}