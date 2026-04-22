using System;
using System.Collections.Generic;
using System.Linq;
using StellarDotnetSdk.Xdr;
using ResultCodeEnum = StellarDotnetSdk.Xdr.TransactionResultCode.TransactionResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

/// <summary>
///     Represents the result of a Stellar transaction, indicating whether it succeeded or the reason for failure.
/// </summary>
public abstract class TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionResult" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    protected TransactionResult(string feeCharged)
    {
        FeeCharged = feeCharged;
    }

    /// <summary>
    ///     Actual fee charged for the transaction.
    /// </summary>
    public string? FeeCharged { get; }

    /// <summary>
    ///     Indicates whether the transaction was successful.
    /// </summary>
    public virtual bool IsSuccess => false;

    /// <summary>
    ///     Creates a new TransactionResult object from the given TransactionResult XDR base64 string.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns>LedgerEntry object</returns>
    public static TransactionResult FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var result = Xdr.TransactionResult.Decode(new XdrDataInputStream(bytes));
        return FromXdr(result);
    }

    private static TransactionResult FromXdr(Xdr.TransactionResult result)
    {
        var feeCharged = Amount.FromXdr(result.FeeCharged.InnerValue);

        return result.Result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.txSUCCESS => new TransactionResultSuccess(
                feeCharged,
                result.Result.Results.Select(OperationResult.FromXdr).ToList()),
            ResultCodeEnum.txFAILED => new TransactionResultFailed(
                feeCharged,
                result.Result.Results.Select(OperationResult.FromXdr).ToList()),
            ResultCodeEnum.txTOO_EARLY => new TransactionResultTooEarly(feeCharged),
            ResultCodeEnum.txTOO_LATE => new TransactionResultTooLate(feeCharged),
            ResultCodeEnum.txMISSING_OPERATION => new TransactionResultMissingOperation(feeCharged),
            ResultCodeEnum.txBAD_SEQ => new TransactionResultBadSeq(feeCharged),
            ResultCodeEnum.txBAD_AUTH => new TransactionResultBadAuth(feeCharged),
            ResultCodeEnum.txINSUFFICIENT_BALANCE => new TransactionResultInsufficientBalance(feeCharged),
            ResultCodeEnum.txNO_ACCOUNT => new TransactionResultNoAccount(feeCharged),
            ResultCodeEnum.txINSUFFICIENT_FEE => new TransactionResultInsufficientFee(feeCharged),
            ResultCodeEnum.txBAD_AUTH_EXTRA => new TransactionResultBadAuthExtra(feeCharged),
            ResultCodeEnum.txINTERNAL_ERROR => new TransactionResultInternalError(feeCharged),
            ResultCodeEnum.txFEE_BUMP_INNER_SUCCESS => new FeeBumpTransactionResultSuccess(
                feeCharged,
                InnerTransactionResultPair.FromXdr(result.Result.InnerResultPair)),
            ResultCodeEnum.txFEE_BUMP_INNER_FAILED => new FeeBumpTransactionResultFailed(
                feeCharged,
                InnerTransactionResultPair.FromXdr(result.Result.InnerResultPair)),
            ResultCodeEnum.txNOT_SUPPORTED => new TransactionResultNotSupported(feeCharged),
            ResultCodeEnum.txBAD_SPONSORSHIP => new TransactionResultBadSponsorship(feeCharged),
            ResultCodeEnum.txBAD_MIN_SEQ_AGE_OR_GAP => new TransactionResultBadMinSeqAgeOrGap(feeCharged),
            ResultCodeEnum.txMALFORMED => new TransactionResultMalformed(feeCharged),
            ResultCodeEnum.txSOROBAN_INVALID => new TransactionResultSorobanInvalid(feeCharged),
            ResultCodeEnum.txFROZEN_KEY_ACCESSED => new TransactionResultFrozenKeyAccessed(feeCharged),
            _ => throw new SystemException("Unknown TransactionResult type"),
        };
    }
}

/// <summary>
///     The ledger closeTime was before the minTime value in the transaction's time bounds.
/// </summary>
public class TransactionResultTooEarly : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionResultTooEarly" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    public TransactionResultTooEarly(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     The ledger closeTime was after the maxTime value in the transaction's time bounds.
/// </summary>
public class TransactionResultTooLate : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionResultTooLate" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    public TransactionResultTooLate(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     One of the operations failed (none were applied).
/// </summary>
public class TransactionResultFailed : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionResultFailed" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    /// <param name="results">The list of individual operation results.</param>
    public TransactionResultFailed(string feeCharged, IList<OperationResult> results) : base(feeCharged)
    {
        Results = results;
    }

    /// <summary>
    ///     The list of individual operation results. At least one operation will have failed.
    /// </summary>
    public IList<OperationResult> Results { get; }
}

/// <summary>
///     All operations succeeded.
/// </summary>
public class TransactionResultSuccess : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionResultSuccess" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    /// <param name="results">The list of individual operation results.</param>
    public TransactionResultSuccess(string feeCharged, List<OperationResult> results) : base(feeCharged)
    {
        Results = results;
    }

    /// <inheritdoc />
    public override bool IsSuccess => true;

    /// <summary>
    ///     The list of individual operation results. All operations succeeded.
    /// </summary>
    public List<OperationResult> Results { get; }
}

/// <summary>
///     Fee would bring account below reserve.
/// </summary>
public class TransactionResultInsufficientBalance : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionResultInsufficientBalance" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    public TransactionResultInsufficientBalance(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     Too few valid signatures or invalid network.
/// </summary>
public class TransactionResultBadAuth : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionResultBadAuth" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    public TransactionResultBadAuth(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     TODO Summary not available on stellar.org
/// </summary>
public class TransactionResultSorobanInvalid : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionResultSorobanInvalid" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    public TransactionResultSorobanInvalid(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     The transaction type is not supported.
/// </summary>
public class TransactionResultNotSupported : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionResultNotSupported" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    public TransactionResultNotSupported(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     Source account not found.
/// </summary>
public class TransactionResultNoAccount : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionResultNoAccount" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    public TransactionResultNoAccount(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     No operation was specified.
/// </summary>
public class TransactionResultMissingOperation : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionResultMissingOperation" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    public TransactionResultMissingOperation(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     TODO Summary not available on stellar.org
/// </summary>
public class TransactionResultMalformed : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionResultMalformed" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    public TransactionResultMalformed(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     An unknown error occurred.
/// </summary>
public class TransactionResultInternalError : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionResultInternalError" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    public TransactionResultInternalError(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     Fee is too small.
/// </summary>
public class TransactionResultInsufficientFee : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionResultInsufficientFee" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    public TransactionResultInsufficientFee(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     The sponsorship is not confirmed.
/// </summary>
public class TransactionResultBadSponsorship : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionResultBadSponsorship" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    public TransactionResultBadSponsorship(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     Sequence number does not match source account.
/// </summary>
public class TransactionResultBadSeq : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionResultBadSeq" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    public TransactionResultBadSeq(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     TODO Summary not available on stellar.org
/// </summary>
public class TransactionResultBadMinSeqAgeOrGap : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionResultBadMinSeqAgeOrGap" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    public TransactionResultBadMinSeqAgeOrGap(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     Unused signatures attached to the transaction.
/// </summary>
public class TransactionResultBadAuthExtra : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionResultBadAuthExtra" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    public TransactionResultBadAuthExtra(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     A frozen ledger key was accessed by one of the transaction's operations.
/// </summary>
public class TransactionResultFrozenKeyAccessed : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionResultFrozenKeyAccessed" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    public TransactionResultFrozenKeyAccessed(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     One of the operations in the inner transaction failed (none were applied).
/// </summary>
public class FeeBumpTransactionResultFailed : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FeeBumpTransactionResultFailed" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    /// <param name="innerResultPair">The inner transaction result pair.</param>
    public FeeBumpTransactionResultFailed(string feeCharged, InnerTransactionResultPair innerResultPair) :
        base(feeCharged)
    {
        InnerResultPair = innerResultPair;
    }

    /// <summary>
    ///     The inner transaction result pair containing the inner transaction hash and its result.
    /// </summary>
    public InnerTransactionResultPair InnerResultPair { get; }
}

/// <summary>
///     All operations in the inner transaction succeeded.
/// </summary>
public class FeeBumpTransactionResultSuccess : TransactionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FeeBumpTransactionResultSuccess" /> class.
    /// </summary>
    /// <param name="feeCharged">The actual fee charged for the transaction.</param>
    /// <param name="innerResultPair">The inner transaction result pair.</param>
    public FeeBumpTransactionResultSuccess(string feeCharged, InnerTransactionResultPair innerResultPair) :
        base(feeCharged)
    {
        InnerResultPair = innerResultPair;
    }

    /// <inheritdoc />
    public override bool IsSuccess => true;

    /// <summary>
    ///     The inner transaction result pair containing the inner transaction hash and its result.
    /// </summary>
    public InnerTransactionResultPair InnerResultPair { get; }
}

/// <summary>
///     Represents the result pair of an inner transaction within a fee bump transaction, containing the transaction hash
///     and its result.
/// </summary>
public class InnerTransactionResultPair
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InnerTransactionResultPair" /> class.
    /// </summary>
    /// <param name="transactionHash">The hash of the inner transaction.</param>
    /// <param name="result">The result of the inner transaction.</param>
    public InnerTransactionResultPair(string transactionHash, TransactionResult result)
    {
        TransactionHash = transactionHash;
        Result = result;
    }

    /// <summary>
    ///     The hash of the inner transaction (base64 encoded).
    /// </summary>
    public string TransactionHash { get; }

    /// <summary>
    ///     The result of the inner transaction.
    /// </summary>
    public TransactionResult Result { get; }

    /// <summary>
    ///     Creates a new <see cref="InnerTransactionResultPair" /> from the given XDR representation.
    /// </summary>
    /// <param name="result">The XDR inner transaction result pair.</param>
    /// <returns>A new <see cref="InnerTransactionResultPair" /> instance.</returns>
    public static InnerTransactionResultPair FromXdr(Xdr.InnerTransactionResultPair result)
    {
        var writer = new XdrDataOutputStream();
        InnerTransactionResult.Encode(writer, result.Result);
        var xdrTransaction = Convert.ToBase64String(writer.ToArray());

        return new InnerTransactionResultPair(
            Convert.ToBase64String(result.TransactionHash.InnerValue),
            TransactionResult.FromXdrBase64(xdrTransaction));
    }
}