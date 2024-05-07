using System;
using System.Linq;
using Asset = StellarDotnetSdk.Assets.Asset;
using ResultCodeEnum = StellarDotnetSdk.Xdr.PathPaymentStrictReceiveResultCode.PathPaymentStrictReceiveResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class PathPaymentStrictReceiveResult : OperationResult
{
    public static PathPaymentStrictReceiveResult FromXdr(Xdr.PathPaymentStrictReceiveResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.PATH_PAYMENT_STRICT_RECEIVE_SUCCESS
                => new PathPaymentStrictReceiveSuccess(
                    result.Success.Offers.Select(ClaimAtom.FromXdr).ToArray(),
                    SimplePaymentResult.FromXdr(result.Success.Last)),
            ResultCodeEnum.PATH_PAYMENT_STRICT_RECEIVE_MALFORMED
                => new PathPaymentStrictReceiveMalformed(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_RECEIVE_UNDERFUNDED
                => new PathPaymentStrictReceiveUnderfunded(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_RECEIVE_SRC_NO_TRUST
                => new PathPaymentStrictReceiveSrcNoTrust(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_RECEIVE_SRC_NOT_AUTHORIZED
                => new PathPaymentStrictReceiveSrcNotAuthorized(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_RECEIVE_NO_DESTINATION
                => new PathPaymentStrictReceiveNoDestination(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_RECEIVE_NO_TRUST
                => new PathPaymentStrictReceiveNoTrust(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_RECEIVE_NOT_AUTHORIZED
                => new PathPaymentStrictReceiveNotAuthorized(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_RECEIVE_LINE_FULL
                => new PathPaymentStrictReceiveLineFull(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_RECEIVE_NO_ISSUER
                => new PathPaymentStrictReceiveNoIssuer(Asset.FromXdr(result.NoIssuer)),
            ResultCodeEnum.PATH_PAYMENT_STRICT_RECEIVE_TOO_FEW_OFFERS
                => new PathPaymentStrictReceiveTooFewOffers(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_RECEIVE_OFFER_CROSS_SELF
                => new PathPaymentStrictReceiveOfferCrossSelf(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_RECEIVE_OVER_SENDMAX
                => new PathPaymentStrictReceiveOverSendmax(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown PathPaymentResult type.")
        };
    }
}

public class PathPaymentStrictReceiveSuccess : PathPaymentStrictReceiveResult
{
    public PathPaymentStrictReceiveSuccess(ClaimAtom[] offers, SimplePaymentResult last)
    {
        Offers = offers;
        Last = last;
    }

    public override bool IsSuccess => true;

    /// <summary>
    ///     Offers claimed in this payment.
    /// </summary>
    public ClaimAtom[] Offers { get; }

    /// <summary>
    ///     Payment result.
    /// </summary>
    public SimplePaymentResult Last { get; }
}

/// <summary>
///     The input to this path payment is invalid.
/// </summary>
public class PathPaymentStrictReceiveMalformed : PathPaymentStrictReceiveResult;

/// <summary>
///     The source account (sender) does not have enough funds to send and still satisfy its selling liabilities. Note that
///     if sending XLM then the sender must additionally maintain its minimum XLM reserve.
/// </summary>
public class PathPaymentStrictReceiveUnderfunded : PathPaymentStrictReceiveResult;

/// <summary>
///     The source account does not trust the issuer of the asset it is trying to send.
/// </summary>
public class PathPaymentStrictReceiveSrcNoTrust : PathPaymentStrictReceiveResult;

/// <summary>
///     The source account is not authorized to send this payment.
/// </summary>
public class PathPaymentStrictReceiveSrcNotAuthorized : PathPaymentStrictReceiveResult;

/// <summary>
///     The destination account does not exist.
/// </summary>
public class PathPaymentStrictReceiveNoDestination : PathPaymentStrictReceiveResult;

/// <summary>
///     The destination account does not trust the issuer of the asset being sent. For more, see the Assets section.
/// </summary>
public class PathPaymentStrictReceiveNoTrust : PathPaymentStrictReceiveResult;

/// <summary>
///     The destination account is not authorized by the asset's issuer to hold the asset.
/// </summary>
public class PathPaymentStrictReceiveNotAuthorized : PathPaymentStrictReceiveResult;

/// <summary>
///     The destination account does not have sufficient limits to receive destination amount and still satisfy its buying
///     liabilities.
/// </summary>
public class PathPaymentStrictReceiveLineFull : PathPaymentStrictReceiveResult;

/// <summary>
///     TODO Missing on official docs
/// </summary>
public class PathPaymentStrictReceiveNoIssuer : PathPaymentStrictReceiveResult
{
    public PathPaymentStrictReceiveNoIssuer(Asset noIssuer)
    {
        NoIssuer = noIssuer;
    }

    /// <summary>
    ///     The asset that caused the error.
    /// </summary>
    public Asset NoIssuer { get; }
}

/// <summary>
///     There is no path of offers connecting the send asset and destination asset. Stellar only considers paths of length
///     5 or shorter.
/// </summary>
public class PathPaymentStrictReceiveTooFewOffers : PathPaymentStrictReceiveResult;

/// <summary>
///     The payment would cross one of its own offers.
/// </summary>
public class PathPaymentStrictReceiveOfferCrossSelf : PathPaymentStrictReceiveResult;

/// <summary>
///     The paths that could send destination amount of destination asset would exceed send max.
/// </summary>
public class PathPaymentStrictReceiveOverSendmax : PathPaymentStrictReceiveResult;