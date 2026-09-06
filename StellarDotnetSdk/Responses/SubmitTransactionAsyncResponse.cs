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
    /// <remarks>
    ///     The type-level <see cref="JsonConverterAttribute" /> is the third and weakest of the three places the
    ///     strict converter is attached, and it covers the case the other two miss: the enum travelling on its
    ///     own — a caller's own DTO field, a persisted status column, a queue message — under a
    ///     <see cref="System.Text.Json.JsonSerializerOptions" /> that registers no converter for it. Without it
    ///     the built-in enum handling maps a bare integer by ordinal, and ordinal 0 here is
    ///     <see cref="PENDING" />, the most optimistic of the four. See
    ///     <see cref="Requests.SorobanRpc.EventFilterType" /> for the full three-tier resolution order.
    /// </remarks>
    [JsonConverter(typeof(SubmitTransactionAsyncStatusJsonConverter))]
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