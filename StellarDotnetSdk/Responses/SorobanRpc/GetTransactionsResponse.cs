namespace StellarDotnetSdk.Responses.SorobanRpc;

public class GetTransactionsResponse
{
    public GetTransactionsResponse(
        TransactionInfo[]? transactions,
        long latestLedger,
        long latestLedgerCloseTimestamp,
        long oldestLedger,
        long oldestLedgerCloseTimestamp)
    {
        Transactions = transactions;
        LatestLedger = latestLedger;
        LatestLedgerCloseTimestamp = latestLedgerCloseTimestamp;
        OldestLedger = oldestLedger;
        OldestLedgerCloseTimestamp = oldestLedgerCloseTimestamp;
    }

    public TransactionInfo[]? Transactions { get; }

    /// <summary>
    ///     The sequence number of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    public long LatestLedger { get; }

    /// <summary>
    ///     The unix timestamp of the close time of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    public long LatestLedgerCloseTimestamp { get; }

    /// <summary>
    ///     The sequence number of the oldest ledger ingested by Soroban RPC at the time it handled the request.
    /// </summary>
    public long OldestLedger { get; }

    /// <summary>
    ///     The unix timestamp of the close time of the oldest ledger ingested by Soroban RPC at the time it handled the
    ///     request.
    /// </summary>
    public long OldestLedgerCloseTimestamp { get; }
}