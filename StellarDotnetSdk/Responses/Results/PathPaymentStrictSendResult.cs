using System;
using System.Linq;
using Asset = StellarDotnetSdk.Assets.Asset;
using ResultCodeEnum = StellarDotnetSdk.Xdr.PathPaymentStrictSendResultCode.PathPaymentStrictSendResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class PathPaymentStrictSendResult : OperationResult
{
    public static PathPaymentStrictSendResult FromXdr(Xdr.PathPaymentStrictSendResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.PATH_PAYMENT_STRICT_SEND_SUCCESS
                => new PathPaymentStrictSendSuccess(
                    result.Success.Offers.Select(ClaimAtom.FromXdr).ToArray(),
                    SimplePaymentResult.FromXdr(result.Success.Last)),
            ResultCodeEnum.PATH_PAYMENT_STRICT_SEND_MALFORMED
                => new PathPaymentStrictSendMalformed(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_SEND_UNDERFUNDED
                => new PathPaymentStrictSendUnderfunded(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_SEND_SRC_NO_TRUST
                => new PathPaymentStrictSendSrcNoTrust(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_SEND_SRC_NOT_AUTHORIZED
                => new PathPaymentStrictSendSrcNotAuthorized(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_SEND_NO_DESTINATION
                => new PathPaymentStrictSendNoDestination(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_SEND_NO_TRUST
                => new PathPaymentStrictSendNoTrust(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_SEND_NOT_AUTHORIZED
                => new PathPaymentStrictSendNotAuthorized(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_SEND_LINE_FULL
                => new PathPaymentStrictSendLineFull(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_SEND_NO_ISSUER
                => new PathPaymentStrictSendNoIssuer(Asset.FromXdr(result.NoIssuer)),
            ResultCodeEnum.PATH_PAYMENT_STRICT_SEND_TOO_FEW_OFFERS
                => new PathPaymentStrictSendTooFewOffers(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_SEND_OFFER_CROSS_SELF
                => new PathPaymentStrictSendOfferCrossSelf(),
            ResultCodeEnum.PATH_PAYMENT_STRICT_SEND_UNDER_DESTMIN
                => new PathPaymentStrictSendUnderDestMin(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown PathPaymentResult type.")
        };
    }
}

public class PathPaymentStrictSendSuccess : PathPaymentStrictSendResult
{
    public PathPaymentStrictSendSuccess(ClaimAtom[] offers, SimplePaymentResult last)
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
public class PathPaymentStrictSendMalformed : PathPaymentStrictSendResult;

/// <summary>
///     The source account (sender) does not have enough funds to send and still satisfy its selling liabilities. Note that
///     if sending XLM then the sender must additionally maintain its minimum XLM reserve.
/// </summary>
public class PathPaymentStrictSendUnderfunded : PathPaymentStrictSendResult;

/// <summary>
///     The source account does not trust the issuer of the asset it is trying to send.
/// </summary>
public class PathPaymentStrictSendSrcNoTrust : PathPaymentStrictSendResult;

/// <summary>
///     The source account is not authorized to send this payment.
/// </summary>
public class PathPaymentStrictSendSrcNotAuthorized : PathPaymentStrictSendResult;

/// <summary>
///     The destination account does not exist.
/// </summary>
public class PathPaymentStrictSendNoDestination : PathPaymentStrictSendResult;

/// <summary>
///     The destination account does not trust the issuer of the asset being sent. For more, see the Assets section.
/// </summary>
public class PathPaymentStrictSendNoTrust : PathPaymentStrictSendResult;

/// <summary>
///     The destination account is not authorized by the asset's issuer to hold the asset.
/// </summary>
public class PathPaymentStrictSendNotAuthorized : PathPaymentStrictSendResult;

/// <summary>
///     The destination account does not have sufficient limits to receive destination amount and still satisfy its buying
///     liabilities.
/// </summary>
public class PathPaymentStrictSendLineFull : PathPaymentStrictSendResult;

/// <summary>
///     There is no path of offers connecting the send asset and destination asset. Stellar only considers paths of length
///     5 or shorter.
/// </summary>
public class PathPaymentStrictSendTooFewOffers : PathPaymentStrictSendResult;

/// <summary>
///     TODO Missing on official docs
/// </summary>
public class PathPaymentStrictSendNoIssuer : PathPaymentStrictSendResult
{
    public PathPaymentStrictSendNoIssuer(Asset noIssuer)
    {
        NoIssuer = noIssuer;
    }

    /// <summary>
    ///     The asset that caused the error.
    /// </summary>
    public Asset NoIssuer { get; }
}

/// <summary>
///     The payment would cross one of its own offers.
/// </summary>
public class PathPaymentStrictSendOfferCrossSelf : PathPaymentStrictSendResult;

/// <summary>
///     The paths that could send destination amount of destination asset would fall short of destination min.
/// </summary>
public class PathPaymentStrictSendUnderDestMin : PathPaymentStrictSendResult;