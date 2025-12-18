using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
///     Unit tests for allow trust result types.
/// </summary>
[TestClass]
public class AllowTrustResultTest
{
    /// <summary>
    ///     Verifies that AllowTrustSuccess result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAllowTrustSuccessXdr_ReturnsAllowTrustSuccess()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAHAAAAAAAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(AllowTrustSuccess), true);
    }

    /// <summary>
    ///     Verifies that AllowTrustMalformed result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAllowTrustMalformedXdr_ReturnsAllowTrustMalformed()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAH/////wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(AllowTrustMalformed), false);
    }

    /// <summary>
    ///     Verifies that AllowTrustNoTrustline result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAllowTrustNoTrustlineXdr_ReturnsAllowTrustNoTrustline()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAH/////gAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(AllowTrustNoTrustline), false);
    }

    /// <summary>
    ///     Verifies that AllowTrustNotRequired result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAllowTrustNotRequiredXdr_ReturnsAllowTrustNotRequired()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAH/////QAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(AllowTrustNotRequired), false);
    }

    /// <summary>
    ///     Verifies that AllowTrustCantRevoke result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAllowTrustCantRevokeXdr_ReturnsAllowTrustCantRevoke()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAH/////AAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(AllowTrustCantRevoke), false);
    }

    /// <summary>
    ///     Verifies that AllowTrustSelfNotAllowed result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAllowTrustSelfNotAllowedXdr_ReturnsAllowTrustSelfNotAllowed()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAH////+wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(AllowTrustSelfNotAllowed), false);
    }
}