using Newtonsoft.Json;
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

    [JsonProperty(PropertyName = "error_result_xdr")]
    private string? _errorResultXdr;

    [JsonProperty(PropertyName = "hash")] public string? Hash { get; init; }

    /// <summary>
    ///     Status of the transaction submission.
    /// </summary>
    [JsonProperty(PropertyName = "tx_status")]
    public TransactionStatus TxStatus { get; init; }

    /// <summary>
    ///     Present only if the submission status <c>TxStatus</c> is an ERROR.
    /// </summary>
    public TransactionResult? ErrorResult =>
        _errorResultXdr != null ? TransactionResult.FromXdrBase64(_errorResultXdr) : null;
}