using System;
using ResultCodeEnum =
    StellarDotnetSdk.Xdr.EndSponsoringFutureReservesResultCode.EndSponsoringFutureReservesResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

/// <summary>
///     Represents the result of an end sponsoring future reserves operation.
/// </summary>
public class EndSponsoringFutureReservesResult : OperationResult
{
    /// <summary>
    ///     Creates the appropriate <see cref="EndSponsoringFutureReservesResult" /> subclass from the given XDR representation.
    /// </summary>
    /// <param name="result">The XDR end sponsoring future reserves result.</param>
    /// <returns>An <see cref="EndSponsoringFutureReservesResult" /> instance representing the operation outcome.</returns>
    public static EndSponsoringFutureReservesResult FromXdr(Xdr.EndSponsoringFutureReservesResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.END_SPONSORING_FUTURE_RESERVES_NOT_SPONSORED
                => new EndSponsoringFutureReservesNotSponsored(),
            ResultCodeEnum.END_SPONSORING_FUTURE_RESERVES_SUCCESS
                => new EndSponsoringFutureReservesSuccess(),
            _ => throw new ArgumentOutOfRangeException(nameof(result),
                "Unknown EndSponsoringFutureReservesResult type."),
        };
    }
}

/// <summary>
///     Represents a successful end sponsoring future reserves operation result.
/// </summary>
public class EndSponsoringFutureReservesSuccess : EndSponsoringFutureReservesResult
{
    /// <inheritdoc />
    public override bool IsSuccess => true;
}

/// <summary>
///     Source account is not sponsored.
/// </summary>
public class EndSponsoringFutureReservesNotSponsored : EndSponsoringFutureReservesResult;