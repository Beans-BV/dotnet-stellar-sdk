using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses.Results;

public class BeginSponsoringFutureReservesResult : OperationResult
{
    public static BeginSponsoringFutureReservesResult FromXdr(Xdr.BeginSponsoringFutureReservesResult result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case BeginSponsoringFutureReservesResultCode.BeginSponsoringFutureReservesResultCodeEnum
                .BEGIN_SPONSORING_FUTURE_RESERVES_ALREADY_SPONSORED:
                return new BeginSponsoringFutureReservesAlreadySponsored();
            case BeginSponsoringFutureReservesResultCode.BeginSponsoringFutureReservesResultCodeEnum
                .BEGIN_SPONSORING_FUTURE_RESERVES_MALFORMED:
                return new BeginSponsoringFutureReservesMalformed();
            case BeginSponsoringFutureReservesResultCode.BeginSponsoringFutureReservesResultCodeEnum
                .BEGIN_SPONSORING_FUTURE_RESERVES_RECURSIVE:
                return new BeginSponsoringFutureReservesRecursive();
            case BeginSponsoringFutureReservesResultCode.BeginSponsoringFutureReservesResultCodeEnum
                .BEGIN_SPONSORING_FUTURE_RESERVES_SUCCESS:
                return new BeginSponsoringFutureReservesSuccess();
            default:
                throw new SystemException("Unknown BeginSponsoringFutureReserves type");
        }
    }
}