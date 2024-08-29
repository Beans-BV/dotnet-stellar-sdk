namespace StellarDotnetSdk.Requests.SorobanRpc;

public class GetTransactionsRequest
{
    /// <summary>
    ///     Ledger sequence number to start fetching responses from (inclusive). This method will return an error if
    ///     startLedger is less than the oldest ledger stored in this node, or greater than the latest ledger seen by this
    ///     node. If a cursor is included in the request, startLedger must be omitted.
    /// </summary>
    public long? StartLedger { get; set; }

    public PaginationOptions? Pagination { get; set; }
}