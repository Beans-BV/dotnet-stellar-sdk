using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using StellarDotnetSdk.Xdr;
using TransactionResult = StellarDotnetSdk.Responses.Results.TransactionResult;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class SubmitTransactionResponse : Response
{
    [JsonProperty(PropertyName = "envelope_xdr")]
    private string _envelopeXdr;

    [JsonProperty(PropertyName = "result_xdr")]
    private string _resultXdr;

    [JsonProperty(PropertyName = "hash")] public string Hash { get; init; }

    [JsonProperty(PropertyName = "ledger")]
    public uint? Ledger { get; init; }

    public string EnvelopeXdr => IsSuccess ? _envelopeXdr : SubmitTransactionResponseExtras.EnvelopeXdr;

    public string ResultXdr => IsSuccess ? _resultXdr : SubmitTransactionResponseExtras.ResultXdr;

    public TransactionResult Result =>
        TransactionResult.FromXdrBase64(IsSuccess ? _resultXdr : SubmitTransactionResponseExtras.ResultXdr);

    [JsonProperty(PropertyName = "extras")]
    public Extras SubmitTransactionResponseExtras { get; init; }

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
        if (!IsSuccess) return null;

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

        if (result.Result.Results[position].Tr.Discriminant.InnerValue !=
            OperationType.OperationTypeEnum.MANAGE_SELL_OFFER) return null;

        if (result.Result.Results[0].Tr.ManageSellOfferResult.Success.Offer.Offer == null) return null;

        return result.Result.Results[0].Tr.ManageSellOfferResult.Success.Offer.Offer.OfferID.InnerValue;
    }

    /// <summary>
    ///     Additional information returned by a server.
    /// </summary>
    public class Extras
    {
        [JsonProperty(PropertyName = "envelope_xdr")]
        public string EnvelopeXdr { get; init; }

        [JsonProperty(PropertyName = "result_xdr")]
        public string ResultXdr { get; init; }

        [JsonProperty(PropertyName = "result_codes")]
        public ResultCodes ExtrasResultCodes { get; init; }

        /// <summary>
        ///     Contains result codes for this transaction.
        ///     see
        ///     <a href="https://github.com/stellar/horizon/blob/master/src/github.com/stellar/horizon/codes/main.go"
        ///         target="_blank">
        ///         Possible values
        ///     </a>
        /// </summary>
        public class ResultCodes
        {
            [JsonProperty(PropertyName = "transaction")]
            public string TransactionResultCode { get; init; }

            [JsonProperty(PropertyName = "operations")]
            public List<string> OperationsResultCodes { get; init; }
        }
    }
}