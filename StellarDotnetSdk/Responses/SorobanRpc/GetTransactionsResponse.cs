namespace StellarDotnetSdk.Responses.SorobanRpc;

public class GetTransactionsResponse
{
    public TransactionInfo[]? Transactions { get; init; }

    /// <summary>
    ///     The sequence number of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    public long LatestLedger { get; init; }

    /// <summary>
    ///     The unix timestamp of the close time of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    public long LatestLedgerCloseTimestamp { get; init; }

    /// <summary>
    ///     The sequence number of the oldest ledger ingested by Soroban RPC at the time it handled the request.
    /// </summary>
    public long OldestLedger { get; init; }

    /// <summary>
    ///     The unix timestamp of the close time of the oldest ledger ingested by Soroban RPC at the time it handled the
    ///     request.
    /// </summary>
    public long OldestLedgerCloseTimestamp { get; init; }
}