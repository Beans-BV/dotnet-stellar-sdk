namespace StellarDotnetSdk.Responses.SorobanRpc;

public class GetTransactionResponse : TransactionInfo
{
    public GetTransactionResponse(
        TransactionStatus status,
        long latestLedger,
        long latestLedgerCloseTime,
        long oldestLedger,
        long oldestLedgerCloseTime,
        long ledger,
        long createdAt,
        int applicationOrder,
        bool feeBump,
        string? envelopeXdr,
        string? resultXdr,
        string? resultMetaXdr,
        string? txHash,
        Events? events
    )
        : base(status, ledger, createdAt, applicationOrder, feeBump, envelopeXdr, resultXdr, resultMetaXdr, txHash,
            events)
    {
        LatestLedger = latestLedger;
        LatestLedgerCloseTime = latestLedgerCloseTime;
        OldestLedger = oldestLedger;
        OldestLedgerCloseTime = oldestLedgerCloseTime;
    }

    /// <summary>
    ///     The sequence number of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    public long LatestLedger { get; }

    /// <summary>
    ///     The unix timestamp of the close time of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    public long LatestLedgerCloseTime { get; }

    /// <summary>
    ///     The sequence number of the oldest ledger ingested by Soroban RPC at the time it handled the request.
    /// </summary>
    public long OldestLedger { get; }

    /// <summary>
    ///     The unix timestamp of the close time of the oldest ledger ingested by Soroban RPC at the time it handled the
    ///     request.
    /// </summary>
    public long OldestLedgerCloseTime { get; }
}