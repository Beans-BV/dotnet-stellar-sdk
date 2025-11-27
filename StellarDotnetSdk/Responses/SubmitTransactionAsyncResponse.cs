using System.Text.Json.Serialization;
using TransactionResult = StellarDotnetSdk.Responses.Results.TransactionResult;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents the response from an asynchronous transaction submission.
///     Async submission is faster but requires polling for the final status.
/// </summary>
public sealed class SubmitTransactionAsyncResponse : Response
{
    /// <summary>
    ///     Possible statuses for an asynchronously submitted transaction.
    /// </summary>
    public enum TransactionStatus
    {
        /// <summary>
        ///     Transaction has been accepted and is pending inclusion in a ledger.
        /// </summary>
        PENDING,

        /// <summary>
        ///     Transaction was already submitted previously.
        /// </summary>
        DUPLICATE,

        /// <summary>
        ///     Server is overloaded; client should retry later.
        /// </summary>
        TRY_AGAIN_LATER,

        /// <summary>
        ///     Transaction failed validation; see ErrorResult for details.
        /// </summary>
        ERROR,
    }

    [JsonInclude]
    [JsonPropertyName("error_result_xdr")]
    private string? ErrorResultXdr { get; init; }

    /// <summary>
    ///     The hash of the submitted transaction.
    /// </summary>
    [JsonPropertyName("hash")]
    public string? Hash { get; init; }

    /// <summary>
    ///     The status of the transaction submission.
    /// </summary>
    [JsonPropertyName("tx_status")]
    public required TransactionStatus TxStatus { get; init; }

    /// <summary>
    ///     The error result if the submission failed (TxStatus is ERROR).
    ///     Null if the transaction was successfully submitted.
    /// </summary>
    public TransactionResult? ErrorResult =>
        ErrorResultXdr != null ? TransactionResult.FromXdrBase64(ErrorResultXdr) : null;
}