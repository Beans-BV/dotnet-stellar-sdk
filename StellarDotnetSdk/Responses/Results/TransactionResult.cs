using System;
using System.Collections.Generic;
using System.Linq;
using StellarDotnetSdk.Xdr;
using ResultCodeEnum = StellarDotnetSdk.Xdr.TransactionResultCode.TransactionResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public abstract class TransactionResult
{
    protected TransactionResult(string feeCharged)
    {
        FeeCharged = feeCharged;
    }

    /// <summary>
    ///     Actual fee charged for the transaction.
    /// </summary>
    public string? FeeCharged { get; }

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
            _ => throw new SystemException("Unknown TransactionResult type"),
        };
    }
}

public class TransactionResultTooEarly : TransactionResult
{
    public TransactionResultTooEarly(string feeCharged) : base(feeCharged)
    {
    }
}

public class TransactionResultTooLate : TransactionResult
{
    public TransactionResultTooLate(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     One of the operations failed (none were applied).
/// </summary>
public class TransactionResultFailed : TransactionResult
{
    public TransactionResultFailed(string feeCharged, IList<OperationResult> results) : base(feeCharged)
    {
        Results = results;
    }

    public IList<OperationResult> Results { get; }
}

/// <summary>
///     All operations succeeded.
/// </summary>
public class TransactionResultSuccess : TransactionResult
{
    public TransactionResultSuccess(string feeCharged, List<OperationResult> results) : base(feeCharged)
    {
        Results = results;
    }

    public override bool IsSuccess => true;

    public List<OperationResult> Results { get; }
}

/// <summary>
///     Fee would bring account below reserve.
/// </summary>
public class TransactionResultInsufficientBalance : TransactionResult
{
    public TransactionResultInsufficientBalance(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     Too few valid signatures or invalid network.
/// </summary>
public class TransactionResultBadAuth : TransactionResult
{
    public TransactionResultBadAuth(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     TODO Summary not available on stellar.org
/// </summary>
public class TransactionResultSorobanInvalid : TransactionResult
{
    public TransactionResultSorobanInvalid(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     The transaction type is not supported.
/// </summary>
public class TransactionResultNotSupported : TransactionResult
{
    public TransactionResultNotSupported(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     Source account not found.
/// </summary>
public class TransactionResultNoAccount : TransactionResult
{
    public TransactionResultNoAccount(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     No operation was specified.
/// </summary>
public class TransactionResultMissingOperation : TransactionResult
{
    public TransactionResultMissingOperation(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     TODO Summary not available on stellar.org
/// </summary>
public class TransactionResultMalformed : TransactionResult
{
    public TransactionResultMalformed(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     An unknown error occured.
/// </summary>
public class TransactionResultInternalError : TransactionResult
{
    public TransactionResultInternalError(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     Fee is too small.
/// </summary>
public class TransactionResultInsufficientFee : TransactionResult
{
    public TransactionResultInsufficientFee(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     The sponsorship is not confirmed.
/// </summary>
public class TransactionResultBadSponsorship : TransactionResult
{
    public TransactionResultBadSponsorship(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     Sequence number does not match source account.
/// </summary>
public class TransactionResultBadSeq : TransactionResult
{
    public TransactionResultBadSeq(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     TODO Summary not available on stellar.org
/// </summary>
public class TransactionResultBadMinSeqAgeOrGap : TransactionResult
{
    public TransactionResultBadMinSeqAgeOrGap(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     Unused signatures attached to the transaction.
/// </summary>
public class TransactionResultBadAuthExtra : TransactionResult
{
    public TransactionResultBadAuthExtra(string feeCharged) : base(feeCharged)
    {
    }
}

/// <summary>
///     One of the operations in the inner transaction failed (none were applied).
/// </summary>
public class FeeBumpTransactionResultFailed : TransactionResult
{
    public FeeBumpTransactionResultFailed(string feeCharged, InnerTransactionResultPair innerResultPair) :
        base(feeCharged)
    {
        InnerResultPair = innerResultPair;
    }

    public InnerTransactionResultPair InnerResultPair { get; }
}

/// <summary>
///     All operations in the inner transaction succeeded.
/// </summary>
public class FeeBumpTransactionResultSuccess : TransactionResult
{
    public FeeBumpTransactionResultSuccess(string feeCharged, InnerTransactionResultPair innerResultPair) :
        base(feeCharged)
    {
        InnerResultPair = innerResultPair;
    }

    public override bool IsSuccess => true;

    public InnerTransactionResultPair InnerResultPair { get; }
}

public class InnerTransactionResultPair
{
    public InnerTransactionResultPair(string transactionHash, TransactionResult result)
    {
        TransactionHash = transactionHash;
        Result = result;
    }

    public string TransactionHash { get; }

    public TransactionResult Result { get; }

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