using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.SorobanRpc;

public class GetTransactionResponse : TransactionInfo
{
    /// <summary>
    ///     The sequence number of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    [JsonPropertyName("latestLedger")]
    public long LatestLedger { get; init; }

    /// <summary>
    ///     The unix timestamp of the close time of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    [JsonPropertyName("latestLedgerCloseTime")]
    public long LatestLedgerCloseTime { get; init; }

    /// <summary>
    ///     The sequence number of the oldest ledger ingested by Soroban RPC at the time it handled the request.
    /// </summary>
    [JsonPropertyName("oldestLedger")]
    public long OldestLedger { get; init; }

    /// <summary>
    ///     The unix timestamp of the close time of the oldest ledger ingested by Soroban RPC at the time it handled the
    ///     request.
    /// </summary>
    [JsonPropertyName("oldestLedgerCloseTime")]
    public long OldestLedgerCloseTime { get; init; }
}