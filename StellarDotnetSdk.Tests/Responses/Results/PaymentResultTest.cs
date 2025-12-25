using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using PaymentResultCodeEnum = StellarDotnetSdk.Xdr.PaymentResultCode.PaymentResultCodeEnum;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
///     Unit tests for payment result types.
/// </summary>
[TestClass]
public class PaymentResultTest
{
    private const string PaymentSuccessXdr = "AAAAAACYloD/////AAAAAQAAAAAAAAABAAAAAAAAAAA=";
    private const string PaymentMalformedXdr = "AAAAAACYloD/////AAAAAQAAAAAAAAAB/////wAAAAA=";
    private const string PaymentUnderfundedXdr = "AAAAAACYloD/////AAAAAQAAAAAAAAAB/////gAAAAA=";
    private const string PaymentSrcNoTrustXdr = "AAAAAACYloD/////AAAAAQAAAAAAAAAB/////QAAAAA=";
    private const string PaymentSrcNotAuthorizedXdr = "AAAAAACYloD/////AAAAAQAAAAAAAAAB/////AAAAAA=";
    private const string PaymentNoDestinationXdr = "AAAAAACYloD/////AAAAAQAAAAAAAAAB////+wAAAAA=";
    private const string PaymentNoTrustXdr = "AAAAAACYloD/////AAAAAQAAAAAAAAAB////+gAAAAA=";
    private const string PaymentNotAuthorizedXdr = "AAAAAACYloD/////AAAAAQAAAAAAAAAB////+QAAAAA=";
    private const string PaymentLineFullXdr = "AAAAAACYloD/////AAAAAQAAAAAAAAAB////+AAAAAA=";
    private const string PaymentNoIssuerXdr = "AAAAAACYloD/////AAAAAQAAAAAAAAAB////9wAAAAA=";

    /// <summary>
    ///     Verifies that payment result types can be deserialized correctly from XDR.
    /// </summary>
    [DataTestMethod]
    [DataRow(PaymentResultCodeEnum.PAYMENT_SUCCESS, typeof(PaymentSuccess), true, PaymentSuccessXdr,
        DisplayName = "Deserialize_WithPaymentSuccessXdr_ReturnsPaymentSuccess")]
    [DataRow(PaymentResultCodeEnum.PAYMENT_MALFORMED, typeof(PaymentMalformed), false, PaymentMalformedXdr,
        DisplayName = "Deserialize_WithPaymentMalformedXdr_ReturnsPaymentMalformed")]
    [DataRow(PaymentResultCodeEnum.PAYMENT_UNDERFUNDED, typeof(PaymentUnderfunded), false, PaymentUnderfundedXdr,
        DisplayName = "Deserialize_WithPaymentUnderfundedXdr_ReturnsPaymentUnderfunded")]
    [DataRow(PaymentResultCodeEnum.PAYMENT_SRC_NO_TRUST, typeof(PaymentSrcNoTrust), false, PaymentSrcNoTrustXdr,
        DisplayName = "Deserialize_WithPaymentSrcNoTrustXdr_ReturnsPaymentSrcNoTrust")]
    [DataRow(PaymentResultCodeEnum.PAYMENT_SRC_NOT_AUTHORIZED, typeof(PaymentSrcNotAuthorized), false,
        PaymentSrcNotAuthorizedXdr,
        DisplayName = "Deserialize_WithPaymentSrcNotAuthorizedXdr_ReturnsPaymentSrcNotAuthorized")]
    [DataRow(PaymentResultCodeEnum.PAYMENT_NO_DESTINATION, typeof(PaymentNoDestination), false, PaymentNoDestinationXdr,
        DisplayName = "Deserialize_WithPaymentNoDestinationXdr_ReturnsPaymentNoDestination")]
    [DataRow(PaymentResultCodeEnum.PAYMENT_NO_TRUST, typeof(PaymentNoTrust), false, PaymentNoTrustXdr,
        DisplayName = "Deserialize_WithPaymentNoTrustXdr_ReturnsPaymentNoTrust")]
    [DataRow(PaymentResultCodeEnum.PAYMENT_NOT_AUTHORIZED, typeof(PaymentNotAuthorized), false, PaymentNotAuthorizedXdr,
        DisplayName = "Deserialize_WithPaymentNotAuthorizedXdr_ReturnsPaymentNotAuthorized")]
    [DataRow(PaymentResultCodeEnum.PAYMENT_LINE_FULL, typeof(PaymentLineFull), false, PaymentLineFullXdr,
        DisplayName = "Deserialize_WithPaymentLineFullXdr_ReturnsPaymentLineFull")]
    [DataRow(PaymentResultCodeEnum.PAYMENT_NO_ISSUER, typeof(PaymentNoIssuer), false, PaymentNoIssuerXdr,
        DisplayName = "Deserialize_WithPaymentNoIssuerXdr_ReturnsPaymentNoIssuer")]
    public void Deserialize_WithResultCode_ReturnsExpectedType(
        PaymentResultCodeEnum resultCode,
        Type expectedType,
        bool isSuccess,
        string xdrBase64)
    {
        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, expectedType, isSuccess);
    }
}