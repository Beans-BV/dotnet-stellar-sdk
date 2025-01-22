namespace StellarDotnetSdk.Responses.SorobanRpc;

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
    ///     Oldest ledger sequence kept in history.
    /// </summary>
    public long OldestLedger { get; init; }

    /// <summary>
    ///     Maximum retention window configured. A full window state can be determined via: ledgerRetentionWindow =
    ///     latestLedger - oldestLedger + 1.
    /// </summary>
    public long LedgerRetentionWindow { get; init; }
}