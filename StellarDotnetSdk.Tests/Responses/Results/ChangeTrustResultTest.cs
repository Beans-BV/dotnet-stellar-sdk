using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
/// Unit tests for change trust result types.
/// </summary>
[TestClass]
public class ChangeTrustResultTest
{
    /// <summary>
    /// Verifies that ChangeTrustSuccess result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithChangeTrustSuccessXdr_ReturnsChangeTrustSuccess()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAGAAAAAAAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ChangeTrustSuccess), true);
    }

    /// <summary>
    /// Verifies that ChangeTrustMalformed result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithChangeTrustMalformedXdr_ReturnsChangeTrustMalformed()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAG/////wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ChangeTrustMalformed), false);
    }

    /// <summary>
    /// Verifies that ChangeTrustNoIssuer result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithChangeTrustNoIssuerXdr_ReturnsChangeTrustNoIssuer()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAG/////gAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ChangeTrustNoIssuer), false);
    }

    /// <summary>
    /// Verifies that ChangeTrustInvalidLimit result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithChangeTrustInvalidLimitXdr_ReturnsChangeTrustInvalidLimit()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAG/////QAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ChangeTrustInvalidLimit), false);
    }

    /// <summary>
    /// Verifies that ChangeTrustLowReserve result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithChangeTrustLowReserveXdr_ReturnsChangeTrustLowReserve()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAG/////AAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ChangeTrustLowReserve), false);
    }

    /// <summary>
    /// Verifies that ChangeTrustSelfNotAllowed result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithChangeTrustSelfNotAllowedXdr_ReturnsChangeTrustSelfNotAllowed()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAG////+wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ChangeTrustSelfNotAllowed), false);
    }
}