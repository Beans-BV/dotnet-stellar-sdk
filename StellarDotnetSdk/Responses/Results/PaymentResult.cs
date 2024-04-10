using System;
using PaymentResultCodeEnum = StellarDotnetSdk.Xdr.PaymentResultCode.PaymentResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class PaymentResult : OperationResult
{
    public static PaymentResult FromXdr(Xdr.PaymentResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            PaymentResultCodeEnum.PAYMENT_SUCCESS => new PaymentSuccess(),
            PaymentResultCodeEnum.PAYMENT_MALFORMED => new PaymentMalformed(),
            PaymentResultCodeEnum.PAYMENT_UNDERFUNDED => new PaymentUnderfunded(),
            PaymentResultCodeEnum.PAYMENT_SRC_NO_TRUST => new PaymentSrcNoTrust(),
            PaymentResultCodeEnum.PAYMENT_SRC_NOT_AUTHORIZED => new PaymentSrcNotAuthorized(),
            PaymentResultCodeEnum.PAYMENT_NO_DESTINATION => new PaymentNoDestination(),
            PaymentResultCodeEnum.PAYMENT_NO_TRUST => new PaymentNoTrust(),
            PaymentResultCodeEnum.PAYMENT_NOT_AUTHORIZED => new PaymentNotAuthorized(),
            PaymentResultCodeEnum.PAYMENT_LINE_FULL => new PaymentLineFull(),
            PaymentResultCodeEnum.PAYMENT_NO_ISSUER => new PaymentNoIssuer(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown PaymentResult type.")
        };
    }
}

public class PaymentSuccess : PaymentResult
{
    public override bool IsSuccess => true;
}

/// <summary>
///     The input to the payment is invalid.
/// </summary>
public class PaymentMalformed : PaymentResult;

/// <summary>
///     The source account (sender) does not have enough funds to send amount and still satisfy its selling liabilities.
///     Note that if sending XLM then the sender must additionally maintain its minimum XLM reserve.
/// </summary>
public class PaymentUnderfunded : PaymentResult;

/// <summary>
///     The source account does not trust the issuer of the asset it is trying to send.
/// </summary>
public class PaymentSrcNoTrust : PaymentResult;

/// <summary>
///     The source account is not authorized to send this payment.
/// </summary>
public class PaymentSrcNotAuthorized : PaymentResult;

/// <summary>
///     The receiving account does not exist. Note that this error will not be returned if the receiving account is the
///     issuer of asset.
/// </summary>
public class PaymentNoDestination : PaymentResult;

/// <summary>
///     The receiver does not trust the issuer of the asset being sent.
/// </summary>
public class PaymentNoTrust : PaymentResult;

/// <summary>
///     The destination account is not authorized by the asset's issuer to hold the asset.
/// </summary>
public class PaymentNotAuthorized : PaymentResult;

/// <summary>
///     The destination account (receiver) does not have sufficient limits to receive amount and still satisfy its buying
///     liabilities.
/// </summary>
public class PaymentLineFull : PaymentResult;

/// <summary>
///     TODO Missing on official docs
/// </summary>
public class PaymentNoIssuer : PaymentResult;