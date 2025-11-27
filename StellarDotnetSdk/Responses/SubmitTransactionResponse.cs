using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Xdr;
using TransactionResult = StellarDotnetSdk.Responses.Results.TransactionResult;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents the response from a synchronous transaction submission to Horizon.
///     Contains the result of the transaction execution including success/failure status.
/// </summary>
public sealed class SubmitTransactionResponse : Response
{
    /// <summary>
    ///     The XDR-encoded transaction envelope. Not present if the transaction fails.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("envelope_xdr")]
    private string? _envelopeXdr;

    /// <summary>
    ///     The XDR-encoded transaction result. Not present if the transaction fails.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("result_xdr")]
    private string? _resultXdr;

    /// <summary>
    ///     The hash of the submitted transaction.
    /// </summary>
    [JsonPropertyName("hash")]
    public string? Hash { get; init; }

    /// <summary>
    ///     The ledger sequence number in which the transaction was included.
    ///     Null if the transaction failed.
    /// </summary>
    [JsonPropertyName("ledger")]
    public long? Ledger { get; init; }

    /// <summary>
    ///     The XDR-encoded transaction envelope.
    ///     Retrieved from the success response or the extras on failure.
    /// </summary>
    public string? EnvelopeXdr => IsSuccess ? _envelopeXdr : SubmitTransactionResponseExtras?.EnvelopeXdr;

    /// <summary>
    ///     The XDR-encoded transaction result.
    ///     Retrieved from the success response or the extras on failure.
    /// </summary>
    public string? ResultXdr => IsSuccess ? _resultXdr : SubmitTransactionResponseExtras?.ResultXdr;

    /// <summary>
    ///     The parsed transaction result.
    /// </summary>
    public TransactionResult? Result
    {
        get
        {
            var xdr = IsSuccess ? _resultXdr : SubmitTransactionResponseExtras?.ResultXdr;
            return xdr != null ? TransactionResult.FromXdrBase64(xdr) : null;
        }
    }

    /// <summary>
    ///     Additional information about a failed transaction submission.
    /// </summary>
    [JsonPropertyName("extras")]
    public Extras? SubmitTransactionResponseExtras { get; init; }

    /// <summary>
    ///     Whether the transaction was successfully included in a ledger.
    /// </summary>
    public bool IsSuccess => Ledger != null;

    /// <summary>
    ///     Helper method that returns Offer ID for ManageOffer from TransactionResult Xdr.
    ///     This is helpful when you need ID of an offer to update it later.
    /// </summary>
    /// <param name="position">
    ///     Position of ManageOffer operation. If ManageOffer is second operation in this transaction this
    ///     should be equal <code>1</code>.
    /// </param>
    /// <returns>
    ///     Offer ID or <code>null</code> when operation at <code>position</code> is not a ManageOffer operation or error
    ///     has occurred.
    /// </returns>
    public long? GetOfferIdFromResult(int position)
    {
        if (!IsSuccess || ResultXdr == null)
        {
            return null;
        }

        var bytes = Convert.FromBase64String(ResultXdr);
        var xdrInputStream = new XdrDataInputStream(bytes);
        Xdr.TransactionResult result;

        try
        {
            result = Xdr.TransactionResult.Decode(xdrInputStream);
        }
        catch (Exception)
        {
            return null;
        }

        if (result.Result.Results == null ||
            result.Result.Results[position].Tr?.Discriminant.InnerValue !=
            OperationType.OperationTypeEnum.MANAGE_SELL_OFFER)
        {
            return null;
        }

        if (result.Result.Results[0].Tr?.ManageSellOfferResult?.Success?.Offer?.Offer == null)
        {
            return null;
        }

        return result.Result.Results[0].Tr.ManageSellOfferResult.Success.Offer.Offer.OfferID.InnerValue;
    }

    /// <summary>
    ///     Additional information returned by the server, typically on failure.
    /// </summary>
    public sealed class Extras
    {
        /// <summary>
        ///     The XDR-encoded transaction envelope.
        /// </summary>
        [JsonPropertyName("envelope_xdr")]
        public string? EnvelopeXdr { get; init; }

        /// <summary>
        ///     The XDR-encoded transaction result.
        /// </summary>
        [JsonPropertyName("result_xdr")]
        public string? ResultXdr { get; init; }

        /// <summary>
        ///     The result codes for the transaction and its operations.
        /// </summary>
        [JsonPropertyName("result_codes")]
        public ResultCodes? ExtrasResultCodes { get; init; }

        /// <summary>
        ///     Contains result codes for this transaction.
        ///     See
        ///     <a href="https://github.com/stellar/horizon/blob/master/src/github.com/stellar/horizon/codes/main.go"
        ///         target="_blank">
        ///         Possible values
        ///     </a>
        /// </summary>
        public sealed class ResultCodes
        {
            /// <summary>
            ///     The result code for the transaction as a whole.
            /// </summary>
            [JsonPropertyName("transaction")]
            public string? TransactionResultCode { get; init; }

            /// <summary>
            ///     The result codes for each operation in the transaction.
            /// </summary>
            [JsonPropertyName("operations")]
            public List<string>? OperationsResultCodes { get; init; }
        }
    }
}