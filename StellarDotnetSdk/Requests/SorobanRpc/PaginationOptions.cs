namespace StellarDotnetSdk.Requests.SorobanRpc;

/// <summary>
///     Pagination in RPC is similar to pagination in Horizon.
///     See: <a href="https://developers.stellar.org/docs/data/rpc/api-reference/pagination">Pagination</a>
/// </summary>
public class PaginationOptions
{
    /// <summary>
    ///     A unique identifier (specifically, a TOID) that points to a specific location in a collection of responses and is
    ///     pulled from the paging_token value of a record. When a cursor is provided, RPC will not include the element whose
    ///     ID matches the cursor in the response: only elements which appear after the cursor will be included.
    /// </summary>
    public string? Cursor { get; set; }

    /// <summary>
    ///     The maximum number of records returned. For getTransactions, this ranges from 1 to 200 and defaults to 50.
    /// </summary>
    public long? Limit { get; set; }
}