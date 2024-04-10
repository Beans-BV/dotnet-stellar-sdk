using System;
using ResultCodeEnum =
    StellarDotnetSdk.Xdr.EndSponsoringFutureReservesResultCode.EndSponsoringFutureReservesResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class EndSponsoringFutureReservesResult : OperationResult
{
    public static EndSponsoringFutureReservesResult FromXdr(Xdr.EndSponsoringFutureReservesResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.END_SPONSORING_FUTURE_RESERVES_NOT_SPONSORED
                => new EndSponsoringFutureReservesNotSponsored(),
            ResultCodeEnum.END_SPONSORING_FUTURE_RESERVES_SUCCESS
                => new EndSponsoringFutureReservesSuccess(),
            _ => throw new ArgumentOutOfRangeException(nameof(result),
                "Unknown EndSponsoringFutureReservesResult type.")
        };
    }
}

public class EndSponsoringFutureReservesSuccess : EndSponsoringFutureReservesResult
{
    public override bool IsSuccess => true;
}

/// <summary>
///     Source account is not sponsored.
/// </summary>
public class EndSponsoringFutureReservesNotSponsored : EndSponsoringFutureReservesResult;