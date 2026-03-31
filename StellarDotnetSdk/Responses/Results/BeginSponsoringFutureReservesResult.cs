using System;
using ResultCodeEnum =
    StellarDotnetSdk.Xdr.BeginSponsoringFutureReservesResultCode.BeginSponsoringFutureReservesResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

/// <summary>
///     Represents the result of a begin sponsoring future reserves operation.
/// </summary>
public class BeginSponsoringFutureReservesResult : OperationResult
{
    /// <summary>
    ///     Creates the appropriate <see cref="BeginSponsoringFutureReservesResult" /> subclass from the given XDR representation.
    /// </summary>
    /// <param name="result">The XDR begin sponsoring future reserves result.</param>
    /// <returns>A <see cref="BeginSponsoringFutureReservesResult" /> instance representing the operation outcome.</returns>
    public static BeginSponsoringFutureReservesResult FromXdr(Xdr.BeginSponsoringFutureReservesResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.BEGIN_SPONSORING_FUTURE_RESERVES_ALREADY_SPONSORED
                => new BeginSponsoringFutureReservesAlreadySponsored(),
            ResultCodeEnum.BEGIN_SPONSORING_FUTURE_RESERVES_MALFORMED
                => new BeginSponsoringFutureReservesMalformed(),
            ResultCodeEnum.BEGIN_SPONSORING_FUTURE_RESERVES_RECURSIVE
                => new BeginSponsoringFutureReservesRecursive(),
            ResultCodeEnum.BEGIN_SPONSORING_FUTURE_RESERVES_SUCCESS
                => new BeginSponsoringFutureReservesSuccess(),
            _ => throw new ArgumentOutOfRangeException(nameof(result),
                "Unknown BeginSponsoringFutureReservesResult type."),
        };
    }
}

/// <summary>
///     Represents a successful begin sponsoring future reserves operation result.
/// </summary>
public class BeginSponsoringFutureReservesSuccess : BeginSponsoringFutureReservesResult
{
    /// <inheritdoc />
    public override bool IsSuccess => true;
}

/// <summary>
///     Source account is equal to sponsoredID.
/// </summary>
public class BeginSponsoringFutureReservesMalformed : BeginSponsoringFutureReservesResult;

/// <summary>
///     Source account is already sponsoring sponsoredID.
/// </summary>
public class BeginSponsoringFutureReservesAlreadySponsored : BeginSponsoringFutureReservesResult;

/// <summary>
///     Either source account is currently being sponsored, or sponsoredID is sponsoring another account.
/// </summary>
public class BeginSponsoringFutureReservesRecursive : BeginSponsoringFutureReservesResult;