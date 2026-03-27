namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Holds the details of the response of <c>getLedgers()</c>.
/// </summary>
public class GetLedgersResponse
{
    /// <summary>
    ///     An array of ledger details matching the request criteria.
    /// </summary>
    public LedgerInfo[]? Ledgers { get; init; }

    /// <summary>
    ///     The sequence number of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    public long LatestLedger { get; init; }

    /// <summary>
    ///     The unix timestamp of the close time of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    public long LatestLedgerCloseTime { get; init; }

    /// <summary>
    ///     The sequence number of the oldest ledger ingested by Soroban RPC at the time it handled the request.
    /// </summary>
    public long OldestLedger { get; init; }

    /// <summary>
    ///     The unix timestamp of the close time of the oldest ledger ingested by Soroban RPC at the time it handled the
    ///     request.
    /// </summary>
    public long OldestLedgerCloseTime { get; init; }

    /// <summary>
    ///     The cursor corresponding to the last returned ledger. Use this cursor to paginate forward by setting it as the
    ///     cursor in the next request.
    /// </summary>
    public string? Cursor { get; init; }
}