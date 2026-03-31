namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Transaction status and network state. The result will include if the transaction was successfully enqueued, and
///     information about the current ledger.
/// </summary>
/// <seealso href="https://developers.stellar.org/docs/data/apis/rpc/api-reference/methods/sendTransaction" />
public class SendTransactionResponse
{
    /// <summary>
    ///     Indicates the status of a submitted transaction after being processed by the Soroban RPC server.
    /// </summary>
    public enum SendTransactionStatus
    {
        /// <summary>The transaction has been accepted and is pending inclusion in a ledger.</summary>
        PENDING,

        /// <summary>The transaction is a duplicate of an already submitted transaction.</summary>
        DUPLICATE,

        /// <summary>The server is busy; the client should retry the submission later.</summary>
        TRY_AGAIN_LATER,

        /// <summary>The transaction was rejected due to an error.</summary>
        ERROR,
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SendTransactionResponse" /> class.
    /// </summary>
    /// <param name="errorResultXdr">Base64-encoded XDR string of the TransactionResult if status is ERROR.</param>
    /// <param name="hash">The hex-encoded transaction hash.</param>
    /// <param name="latestLedger">The latest ledger sequence number known to the server.</param>
    /// <param name="latestLedgerCloseTime">The close time of the latest ledger.</param>
    /// <param name="status">The status of the submitted transaction.</param>
    public SendTransactionResponse(
        string? errorResultXdr,
        string hash,
        long? latestLedger,
        long? latestLedgerCloseTime,
        SendTransactionStatus status
    )
    {
        ErrorResultXdr = errorResultXdr;
        Hash = hash;
        LatestLedger = latestLedger;
        LatestLedgerCloseTime = latestLedgerCloseTime;
        Status = status;
    }

    /// <summary>
    ///     (optional) If the transaction status is ERROR, this will be a base64 encoded string of the raw TransactionResult
    ///     XDR struct containing details on why stellar-core rejected the transaction.
    /// </summary>
    public string? ErrorResultXdr { get; init; }

    /// <summary>
    ///     The transaction hash (a hex-encoded string).
    /// </summary>
    public string Hash { get; init; }

    /// <summary>
    ///     The sequence number of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    public long? LatestLedger { get; init; }

    /// <summary>
    ///     The unix timestamp of the close time of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    public long? LatestLedgerCloseTime { get; init; }

    /// <summary>
    ///     The current status of the transaction by hash, one of: ERROR, PENDING, DUPLICATE, TRY_AGAIN_LATER.
    ///     ERROR represents the status value returned by stellar-core when an error occurred from submitting a transaction.
    ///     PENDING represents the status value returned by stellar-core when a transaction has been accepted for processing.
    ///     DUPLICATE represents the status value returned by stellar-core when a submitted transaction is a duplicate.
    ///     TRY_AGAIN_LATER represents the status value returned by stellar-core when a submitted transaction was not included
    ///     in the previous 4 ledgers and get banned for being added in the next few ledgers.
    /// </summary>
    public SendTransactionStatus Status { get; init; }
}