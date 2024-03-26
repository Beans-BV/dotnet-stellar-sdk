using System;
using System.Collections.Generic;
using System.Linq;
using stellar_dotnet_sdk.xdr;
using OperationResult = stellar_dotnet_sdk.responses.results.OperationResult;

namespace stellar_dotnet_sdk.responses;

public abstract class TransactionResult
{
    /// <summary>
    ///     Actual fee charged for the transaction.
    /// </summary>
    public string? FeeCharged { get; protected init; }

    public abstract bool IsSuccess { get; } 

    /// <summary>
    ///     Creates a new TransactionResult object from the given TransactionResult XDR base64 string.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns>LedgerEntry object</returns>
    public static TransactionResult FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var result = xdr.TransactionResult.Decode(new XdrDataInputStream(bytes));
        return FromXdr(result);
    }

    public static TransactionResult FromXdr(xdr.TransactionResult result)
    {
        var feeCharged = Amount.FromXdr(result.FeeCharged.InnerValue);

        switch (result.Result.Discriminant.InnerValue)
        {
            case TransactionResultCode.TransactionResultCodeEnum.txSUCCESS:
                return new TransactionResultSuccess(feeCharged,
                    ResultsFromXdr(result.Result.Results));
            case TransactionResultCode.TransactionResultCodeEnum.txFAILED:
                return new TransactionResultFailed(feeCharged,
                    ResultsFromXdr(result.Result.Results));
            case TransactionResultCode.TransactionResultCodeEnum.txTOO_EARLY:
                return new TransactionResultTooEarly
                {
                    FeeCharged = feeCharged
                };
            case TransactionResultCode.TransactionResultCodeEnum.txTOO_LATE:
                return new TransactionResultTooLate
                {
                    FeeCharged = feeCharged
                };
            case TransactionResultCode.TransactionResultCodeEnum.txMISSING_OPERATION:
                return new TransactionResultMissingOperation
                {
                    FeeCharged = feeCharged
                };
            case TransactionResultCode.TransactionResultCodeEnum.txBAD_SEQ:
                return new TransactionResultBadSeq
                {
                    FeeCharged = feeCharged
                };
            case TransactionResultCode.TransactionResultCodeEnum.txBAD_AUTH:
                return new TransactionResultBadAuth
                {
                    FeeCharged = feeCharged
                };
            case TransactionResultCode.TransactionResultCodeEnum.txINSUFFICIENT_BALANCE:
                return new TransactionResultInsufficientBalance
                {
                    FeeCharged = feeCharged
                };
            case TransactionResultCode.TransactionResultCodeEnum.txNO_ACCOUNT:
                return new TransactionResultNoAccount
                {
                    FeeCharged = feeCharged
                };
            case TransactionResultCode.TransactionResultCodeEnum.txINSUFFICIENT_FEE:
                return new TransactionResultInsufficientFee
                {
                    FeeCharged = feeCharged
                };
            case TransactionResultCode.TransactionResultCodeEnum.txBAD_AUTH_EXTRA:
                return new TransactionResultBadAuthExtra
                {
                    FeeCharged = feeCharged
                };
            case TransactionResultCode.TransactionResultCodeEnum.txINTERNAL_ERROR:
                return new TransactionResultInternalError
                {
                    FeeCharged = feeCharged
                };
            case TransactionResultCode.TransactionResultCodeEnum.txFEE_BUMP_INNER_SUCCESS:
                return new FeeBumpTransactionResultSuccess(feeCharged,
                    InnerTransactionResultPair.FromXdr(result.Result.InnerResultPair));
            case TransactionResultCode.TransactionResultCodeEnum.txFEE_BUMP_INNER_FAILED:
                return new FeeBumpTransactionResultFailed(feeCharged,
                    InnerTransactionResultPair.FromXdr(result.Result.InnerResultPair));
            case TransactionResultCode.TransactionResultCodeEnum.txNOT_SUPPORTED:
                return new TransactionResultNotSupported
                {
                    FeeCharged = feeCharged
                };
            case TransactionResultCode.TransactionResultCodeEnum.txBAD_SPONSORSHIP:
                return new TransactionResultBadSponsorship
                {
                    FeeCharged = feeCharged
                };
            case TransactionResultCode.TransactionResultCodeEnum.txBAD_MIN_SEQ_AGE_OR_GAP:
                return new TransactionResultBadMinSeqAgeOrGap
                {
                    FeeCharged = feeCharged
                };
            case TransactionResultCode.TransactionResultCodeEnum.txMALFORMED:
                return new TransactionResultMalformed
                {
                    FeeCharged = feeCharged
                };
            case TransactionResultCode.TransactionResultCodeEnum.txSOROBAN_INVALID:
                return new TransactionResultSorobanInvalid
                {
                    FeeCharged = feeCharged
                };
            default:
                throw new SystemException("Unknown TransactionResult type");
        }
    }

    private static List<OperationResult> ResultsFromXdr(xdr.OperationResult[] xdrResults)
    {
        return xdrResults.Select(OperationResult.FromXdr).ToList();
    }
}