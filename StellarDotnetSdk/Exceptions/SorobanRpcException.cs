using System;
using System.Text.Json;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     The exception that is thrown when the Stellar RPC server answers a request with a JSON-RPC error
///     object (JSON-RPC 2.0 §5.1) instead of a result. Such a response carries no <c>result</c> member and
///     normally arrives with HTTP status 200, so it is distinct from the HTTP-level failures reported by
///     <see cref="ServiceUnavailableException" />, <see cref="TooManyRequestsException" />, and
///     <see cref="Requests.HttpResponseException" />.
/// </summary>
public class SorobanRpcException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SorobanRpcException" /> class.
    /// </summary>
    /// <param name="code">The JSON-RPC error code returned by the server.</param>
    /// <param name="message">The error message returned by the server, if any.</param>
    /// <param name="data">The optional <c>data</c> member of the JSON-RPC error object.</param>
    public SorobanRpcException(int code, string? message, JsonElement? data = null)
        : base(FormatMessage(code, message))
    {
        Code = code;
        ErrorMessage = message;
        ErrorData = data;
    }

    /// <summary>
    ///     The JSON-RPC error code. Codes from -32768 to -32000 are reserved by the JSON-RPC specification
    ///     (for example -32602 <c>Invalid params</c>); Stellar RPC also reuses them for request-scoped errors.
    /// </summary>
    public int Code { get; }

    /// <summary>
    ///     The error message exactly as returned by the server, or null if the response omitted it.
    ///     <see cref="Exception.Message" /> wraps this value together with <see cref="Code" />.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    ///     The optional <c>data</c> member of the JSON-RPC error object, preserved verbatim, or null if the
    ///     response omitted it. JSON-RPC allows any JSON value here, so it is exposed as a
    ///     <see cref="JsonElement" />: call <see cref="JsonElement.ToString" /> for its text form or
    ///     <c>Deserialize&lt;T&gt;()</c> to bind it to a type.
    /// </summary>
    public JsonElement? ErrorData { get; }

    private static string FormatMessage(int code, string? message)
    {
        return string.IsNullOrWhiteSpace(message)
            ? $"The Stellar RPC server returned JSON-RPC error {code} with no message."
            : $"The Stellar RPC server returned JSON-RPC error {code}: {message}";
    }
}
