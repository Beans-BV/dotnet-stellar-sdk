using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses.Results;

public class EndSponsoringFutureReservesResult : OperationResult
{
    public static EndSponsoringFutureReservesResult FromXdr(Xdr.EndSponsoringFutureReservesResult result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case EndSponsoringFutureReservesResultCode.EndSponsoringFutureReservesResultCodeEnum
                .END_SPONSORING_FUTURE_RESERVES_NOT_SPONSORED:
                return new EndSponsoringFutureReservesNotSponsored();
            case EndSponsoringFutureReservesResultCode.EndSponsoringFutureReservesResultCodeEnum
                .END_SPONSORING_FUTURE_RESERVES_SUCCESS:
                return new EndSponsoringFutureReservesSuccess();
            default:
                throw new SystemException("Unknown EndSponsoringFutureReserves type");
        }
    }
}