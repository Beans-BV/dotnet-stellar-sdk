using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Represents the response from the Soroban RPC <c>getHealth</c> method.
///     Contains the health status, ledger information, and retention window of the Soroban RPC server.
/// </summary>
public class GetHealthResponse
{
    /// <summary>
    ///     Health status e.g. "healthy"
    /// </summary>
    public string Status { get; init; }

    /// <summary>
    ///     Most recent known ledger sequence.
    /// </summary>
    public long LatestLedger { get; init; }

    /// <summary>
    ///     The close time of the latest ledger, as a unix timestamp in seconds. <c>null</c> when the server is older
    ///     than RPC v27.1.0 and omits it. Stellar RPC sends it as a quoted number.
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
    public long? LatestLedgerCloseTime { get; init; }

    /// <summary>
    ///     Oldest ledger sequence kept in history.
    /// </summary>
    public long OldestLedger { get; init; }

    /// <summary>
    ///     The close time of the oldest ledger kept in history, as a unix timestamp in seconds. <c>null</c> when the
    ///     server is older than RPC v27.1.0 and omits it. Stellar RPC sends it as a quoted number.
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
    public long? OldestLedgerCloseTime { get; init; }

    /// <summary>
    ///     Maximum retention window configured. A full window state can be determined via: ledgerRetentionWindow =
    ///     latestLedger - oldestLedger + 1.
    /// </summary>
    public long LedgerRetentionWindow { get; init; }
}