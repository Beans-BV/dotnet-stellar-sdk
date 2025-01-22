﻿using System.Text.Json.Serialization;
using TransactionResult = StellarDotnetSdk.Responses.Results.TransactionResult;

namespace StellarDotnetSdk.Responses;

public class SubmitTransactionAsyncResponse : Response
{
    public enum TransactionStatus
    {
        PENDING,
        DUPLICATE,
        TRY_AGAIN_LATER,
        ERROR,
    }

    // TODO: Change to "error_result_xdr" when Horizon is updated to v22.0.0
    [JsonPropertyName("errorResultXdr")]
    [JsonInclude]
    private string? ErrorResultXdr { get; init; }

    [JsonPropertyName("hash")] public string? Hash { get; init; }

    /// <summary>
    ///     Status of the transaction submission.
    /// </summary>
    [JsonPropertyName("tx_status")]
    public TransactionStatus TxStatus { get; init; }

    /// <summary>
    ///     Present only if the submission status <c>TxStatus</c> is an ERROR.
    /// </summary>
    public TransactionResult? ErrorResult =>
        ErrorResultXdr != null ? TransactionResult.FromXdrBase64(ErrorResultXdr) : null;
}