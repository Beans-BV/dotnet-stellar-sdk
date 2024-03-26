using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses.Results;

public class RevokeSponsorshipResult : OperationResult
{
    public static RevokeSponsorshipResult FromXdr(Xdr.RevokeSponsorshipResult result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case RevokeSponsorshipResultCode.RevokeSponsorshipResultCodeEnum.REVOKE_SPONSORSHIP_DOES_NOT_EXIST:
                return new RevokeSponsorshipDoesNotExist();
            case RevokeSponsorshipResultCode.RevokeSponsorshipResultCodeEnum.REVOKE_SPONSORSHIP_LOW_RESERVE:
                return new RevokeSponsorshipLowReserve();
            case RevokeSponsorshipResultCode.RevokeSponsorshipResultCodeEnum.REVOKE_SPONSORSHIP_NOT_SPONSOR:
                return new RevokeSponsorshipNotSponsor();
            case RevokeSponsorshipResultCode.RevokeSponsorshipResultCodeEnum.REVOKE_SPONSORSHIP_ONLY_TRANSFERABLE:
                return new RevokeSponsorshipOnlyTransferable();
            case RevokeSponsorshipResultCode.RevokeSponsorshipResultCodeEnum.REVOKE_SPONSORSHIP_SUCCESS:
                return new RevokeSponsorshipSuccess();
            default:
                throw new SystemException("Unknown RevokeSponsorship type");
        }
    }
}