using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Requests.SorobanRpc;

/// <summary>
///     Represents the request parameters for the Soroban RPC <c>getTransactions</c> method,
///     which retrieves a list of transactions from the network.
/// </summary>
public class GetTransactionsRequest
{
    /// <summary>
    ///     Ledger sequence number to start fetching responses from (inclusive). This method will return an error if
    ///     startLedger is less than the oldest ledger stored in this node, or greater than the latest ledger seen by this
    ///     node. If a cursor is included in the request, startLedger must be omitted.
    /// </summary>
    [JsonPropertyName("startLedger")]
    public long? StartLedger { get; set; }

    /// <summary>
    ///     Pagination options for controlling the cursor position and page size of transaction results.
    /// </summary>
    [JsonPropertyName("pagination")]
    public PaginationOptions? Pagination { get; set; }
}