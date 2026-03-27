using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Represents the response from the Soroban RPC <c>getTransaction</c> method.
///     Extends <see cref="TransactionInfo" /> with additional ledger context about when the request was handled.
/// </summary>
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