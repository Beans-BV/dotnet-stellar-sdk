using System;
using ResultCodeEnum = StellarDotnetSdk.Xdr.AccountMergeResultCode.AccountMergeResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class AccountMergeResult : OperationResult
{
    public static AccountMergeResult FromXdr(Xdr.AccountMergeResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.ACCOUNT_MERGE_SUCCESS
                => new AccountMergeSuccess(Amount.FromXdr(result.SourceAccountBalance.InnerValue)),
            ResultCodeEnum.ACCOUNT_MERGE_MALFORMED
                => new AccountMergeMalformed(),
            ResultCodeEnum.ACCOUNT_MERGE_NO_ACCOUNT
                => new AccountMergeNoAccount(),
            ResultCodeEnum.ACCOUNT_MERGE_IMMUTABLE_SET
                => new AccountMergeImmutableSet(),
            ResultCodeEnum.ACCOUNT_MERGE_HAS_SUB_ENTRIES
                => new AccountMergeHasSubEntries(),
            ResultCodeEnum.ACCOUNT_MERGE_SEQNUM_TOO_FAR
                => new AccountMergeSequenceNumberTooFar(),
            ResultCodeEnum.ACCOUNT_MERGE_DEST_FULL
                => new AccountMergeDestFull(),
            ResultCodeEnum.ACCOUNT_MERGE_IS_SPONSOR
                => new AccountMergeIsSponsor(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown AccountMergeResult type."),
        };
    }
}

/// <summary>
///     Operation successful.
/// </summary>
public class AccountMergeSuccess : AccountMergeResult
{
    public AccountMergeSuccess(string sourceAccountBalance)
    {
        SourceAccountBalance = sourceAccountBalance;
    }

    public override bool IsSuccess => true;

    /// <summary>
    ///     How much got transferred from source account.
    /// </summary>
    public string SourceAccountBalance { get; }
}

/// <summary>
///     The operation is malformed because the source account cannot merge with itself. The destination must be a different
///     account.
/// </summary>
public class AccountMergeMalformed : AccountMergeResult;

/// <summary>
///     The destination account does not exist.
/// </summary>
public class AccountMergeNoAccount : AccountMergeResult;

/// <summary>
///     Source account has <c>AUTH_IMMUTABLE</c> set.
/// </summary>
public class AccountMergeImmutableSet : AccountMergeResult;

/// <summary>
///     The source account has trustlines/offers.
/// </summary>
public class AccountMergeHasSubEntries : AccountMergeResult;

/// <summary>
///     Source's account sequence number is too high.
/// </summary>
public class AccountMergeSequenceNumberTooFar : AccountMergeResult;

/// <summary>
///     The destination account cannot receive the balance of the source account and still satisfy its lumen buying
///     liabilities.
/// </summary>
public class AccountMergeDestFull : AccountMergeResult;

/// <summary>
///     The source account is a sponsor.
/// </summary>
public class AccountMergeIsSponsor : AccountMergeResult;