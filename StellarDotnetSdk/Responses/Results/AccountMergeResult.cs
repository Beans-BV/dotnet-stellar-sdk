using System;
using ResultCodeEnum = StellarDotnetSdk.Xdr.AccountMergeResultCode.AccountMergeResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

/// <summary>
///     Represents the result of an account merge operation.
/// </summary>
public class AccountMergeResult : OperationResult
{
    /// <summary>
    ///     Creates the appropriate <see cref="AccountMergeResult" /> subclass from the given XDR representation.
    /// </summary>
    /// <param name="result">The XDR account merge result.</param>
    /// <returns>An <see cref="AccountMergeResult" /> instance representing the operation outcome.</returns>
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
    /// <summary>
    ///     Initializes a new instance of the <see cref="AccountMergeSuccess" /> class.
    /// </summary>
    /// <param name="sourceAccountBalance">The amount transferred from the source account.</param>
    public AccountMergeSuccess(string sourceAccountBalance)
    {
        SourceAccountBalance = sourceAccountBalance;
    }

    /// <inheritdoc />
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