using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Represents the response from the Soroban RPC <c>getLatestLedger</c> method.
///     Contains the hash, sequence number, protocol version, close time, and encoded header and metadata of the most
///     recent ledger.
/// </summary>
public class GetLatestLedgerResponse
{
    /// <summary>
    ///     Hash identifier of the latest ledger (as a hex-encoded string) known to Stellar RPC at the time it handled the
    ///     request.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    ///     Stellar Core protocol version associated with the latest ledger.
    /// </summary>
    public int ProtocolVersion { get; init; }

    /// <summary>
    ///     The sequence number of the latest ledger known to Stellar RPC at the time it handled the request.
    /// </summary>
    public int Sequence { get; init; }

    /// <summary>
    ///     The close time of the latest ledger, as a unix timestamp in seconds. Stellar RPC sends it as a quoted
    ///     number; <c>null</c> when the server is older than v26 and omits it.
    /// </summary>
    /// <remarks>
    ///     <see cref="JsonNumberHandlingAttribute" /> records the quoted wire form at the declaration, so a caller who
    ///     deserializes this type with their own options reads it without enabling
    ///     <see cref="JsonNumberHandling.AllowReadingFromString" /> globally. Those options must still bind the camelCase
    ///     wire name (for example with <see cref="System.Text.Json.JsonSerializerOptions.PropertyNameCaseInsensitive" />);
    ///     under default case-sensitive options the property stays <c>null</c>. See the remarks on
    ///     <see cref="SimulateTransactionResponse.MinResourceFee" />.
    /// </remarks>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? CloseTime { get; init; }

    /// <summary>
    ///     The base-64 encoded XDR of the latest ledger's header.
    /// </summary>
    public string? HeaderXdr { get; init; }

    /// <summary>
    ///     The base-64 encoded XDR of the latest ledger's close metadata.
    /// </summary>
    public string? MetadataXdr { get; init; }
}