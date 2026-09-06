using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;
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
    // The property-level [JsonConverter] pins the strict wire format whichever options instance the response is
    // deserialized with: System.Text.Json resolves converters property attribute first, then the options'
    // Converters collection, then the type attribute — so this attribute outranks even a catch-all
    // JsonStringEnumConverter registered on the caller's options, which would map bare integers by ordinal.
    // The pin governs the *value* grammar only. The duplicate-property rejection that catches a repeated
    // tx_status is JsonOptions.DefaultOptions' AllowDuplicateProperties setting, which does not travel with
    // the type — under a caller's own options a body that opens with ERROR and repeats tx_status as PENDING
    // still deserializes to PENDING.
    [JsonPropertyName("tx_status")]
    [JsonConverter(typeof(SubmitTransactionAsyncStatusJsonConverter))]
    public required TransactionStatus TxStatus { get; init; }

    /// <summary>
    ///     The error result if the submission failed (TxStatus is ERROR).
    ///     Null if the transaction was successfully submitted.
    /// </summary>
    public TransactionResult? ErrorResult =>
        ErrorResultXdr != null ? TransactionResult.FromXdrBase64(ErrorResultXdr) : null;
}