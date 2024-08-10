using System;
using ResultCodeEnum = StellarDotnetSdk.Xdr.RevokeSponsorshipResultCode.RevokeSponsorshipResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class RevokeSponsorshipResult : OperationResult
{
    public static RevokeSponsorshipResult FromXdr(Xdr.RevokeSponsorshipResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.REVOKE_SPONSORSHIP_DOES_NOT_EXIST => new RevokeSponsorshipDoesNotExist(),
            ResultCodeEnum.REVOKE_SPONSORSHIP_LOW_RESERVE => new RevokeSponsorshipLowReserve(),
            ResultCodeEnum.REVOKE_SPONSORSHIP_NOT_SPONSOR => new RevokeSponsorshipNotSponsor(),
            ResultCodeEnum.REVOKE_SPONSORSHIP_ONLY_TRANSFERABLE => new RevokeSponsorshipOnlyTransferable(),
            ResultCodeEnum.REVOKE_SPONSORSHIP_SUCCESS => new RevokeSponsorshipSuccess(),
            ResultCodeEnum.REVOKE_SPONSORSHIP_MALFORMED => new RevokeSponsorshipMalformed(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown RevokeSponsorshipResult type."),
        };
    }
}

public class RevokeSponsorshipSuccess : RevokeSponsorshipResult
{
    public override bool IsSuccess => true;
}

/// <summary>
///     The ledgerEntry for LedgerKey doesn't exist, the account ID on signer doesn't exist, or the Signer Key doesn't
///     exist on account ID’s account.
/// </summary>
public class RevokeSponsorshipDoesNotExist : RevokeSponsorshipResult;

/// <summary>
///     If the ledgerEntry/signer is sponsored, then the source account must be the sponsor. If the ledgerEntry/signer is
///     not sponsored, the source account must be the owner. This error will be thrown otherwise.
/// </summary>
public class RevokeSponsorshipNotSponsor : RevokeSponsorshipResult;

/// <summary>
///     The sponsored account does not have enough XLM to satisfy the minimum balance increase caused by revoking
///     sponsorship on a ledgerEntry/signer it owns, or the sponsor of the source account doesn’t have enough XLM to
///     satisfy the minimum balance increase caused by sponsoring a transferred ledgerEntry/signer.
/// </summary>
public class RevokeSponsorshipLowReserve : RevokeSponsorshipResult;

/// <summary>
///     Sponsorship cannot be removed from this ledgerEntry. This error will happen if the user tries to remove the
///     sponsorship from a ClaimableBalanceEntry.
/// </summary>
public class RevokeSponsorshipOnlyTransferable : RevokeSponsorshipResult;

/// <summary>
///     One or more of the inputs to the operation was malformed.
/// </summary>
public class RevokeSponsorshipMalformed : RevokeSponsorshipResult;