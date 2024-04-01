using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses.Results;

public class AccountMergeResult : OperationResult
{
    public static AccountMergeResult FromXdr(Xdr.AccountMergeResult result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case AccountMergeResultCode.AccountMergeResultCodeEnum.ACCOUNT_MERGE_SUCCESS:
                return new AccountMergeSuccess
                {
                    SourceAccountBalance = Amount.FromXdr(result.SourceAccountBalance.InnerValue)
                };
            case AccountMergeResultCode.AccountMergeResultCodeEnum.ACCOUNT_MERGE_MALFORMED:
                return new AccountMergeMalformed();
            case AccountMergeResultCode.AccountMergeResultCodeEnum.ACCOUNT_MERGE_NO_ACCOUNT:
                return new AccountMergeNoAccount();
            case AccountMergeResultCode.AccountMergeResultCodeEnum.ACCOUNT_MERGE_IMMUTABLE_SET:
                return new AccountMergeImmutableSet();
            case AccountMergeResultCode.AccountMergeResultCodeEnum.ACCOUNT_MERGE_HAS_SUB_ENTRIES:
                return new AccountMergeHasSubEntries();
            case AccountMergeResultCode.AccountMergeResultCodeEnum.ACCOUNT_MERGE_SEQNUM_TOO_FAR:
                return new AccountMergeSeqnumTooFar();
            case AccountMergeResultCode.AccountMergeResultCodeEnum.ACCOUNT_MERGE_DEST_FULL:
                return new AccountMergeDestFull();
            default:
                throw new SystemException("Unknown AccountMerge type");
        }
    }
}