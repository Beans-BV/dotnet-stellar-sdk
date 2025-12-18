using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
/// Unit tests for payment result types.
/// </summary>
[TestClass]
public class PaymentResultTest
{
    /// <summary>
    /// Verifies that PaymentSuccess result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPaymentSuccessXdr_ReturnsPaymentSuccess()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAABAAAAAAAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PaymentSuccess), true);
    }

    /// <summary>
    /// Verifies that PaymentMalformed result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPaymentMalformedXdr_ReturnsPaymentMalformed()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAB/////wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PaymentMalformed), false);
    }

    /// <summary>
    /// Verifies that PaymentUnderfunded result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPaymentUnderfundedXdr_ReturnsPaymentUnderfunded()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAB/////gAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PaymentUnderfunded), false);
    }

    /// <summary>
    /// Verifies that PaymentSrcNoTrust result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPaymentSrcNoTrustXdr_ReturnsPaymentSrcNoTrust()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAB/////QAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PaymentSrcNoTrust), false);
    }

    /// <summary>
    /// Verifies that PaymentSrcNotAuthorized result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPaymentSrcNotAuthorizedXdr_ReturnsPaymentSrcNotAuthorized()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAB/////AAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PaymentSrcNotAuthorized), false);
    }

    /// <summary>
    /// Verifies that PaymentNoDestination result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPaymentNoDestinationXdr_ReturnsPaymentNoDestination()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAB////+wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PaymentNoDestination), false);
    }

    /// <summary>
    /// Verifies that PaymentNoTrust result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPaymentNoTrustXdr_ReturnsPaymentNoTrust()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAB////+gAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PaymentNoTrust), false);
    }

    /// <summary>
    /// Verifies that PaymentNotAuthorized result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPaymentNotAuthorizedXdr_ReturnsPaymentNotAuthorized()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAB////+QAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PaymentNotAuthorized), false);
    }

    /// <summary>
    /// Verifies that PaymentLineFull result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPaymentLineFullXdr_ReturnsPaymentLineFull()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAB////+AAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PaymentLineFull), false);
    }

    /// <summary>
    /// Verifies that PaymentNoIssuer result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPaymentNoIssuerXdr_ReturnsPaymentNoIssuer()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAB////9wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PaymentNoIssuer), false);
    }
}