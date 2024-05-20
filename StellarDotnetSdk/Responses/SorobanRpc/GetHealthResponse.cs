namespace StellarDotnetSdk.Responses.SorobanRpc;

public class GetHealthResponse
{
    public GetHealthResponse(string status, long latestLedger, long oldestLedger, long ledgerRetentionWindow)
    {
        Status = status;
        LatestLedger = latestLedger;
        OldestLedger = oldestLedger;
        LedgerRetentionWindow = ledgerRetentionWindow;
    }

    /// <summary>
    ///     Health status e.g. "healthy"
    /// </summary>
    public string Status { get; }

    /// <summary>
    ///     Most recent known ledger sequence.
    /// </summary>
    public long LatestLedger { get; }

    /// <summary>
    ///     Oldest ledger sequence kept in history.
    /// </summary>
    public long OldestLedger { get; }

    /// <summary>
    ///     Maximum retention window configured. A full window state can be determined via: ledgerRetentionWindow =
    ///     latestLedger - oldestLedger + 1.
    /// </summary>
    public long LedgerRetentionWindow { get; }
}