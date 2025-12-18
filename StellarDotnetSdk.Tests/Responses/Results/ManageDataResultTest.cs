using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
/// Unit tests for manage data result types.
/// </summary>
[TestClass]
public class ManageDataResultTest
{
    /// <summary>
    /// Verifies that ManageDataSuccess result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageDataSuccessXdr_ReturnsManageDataSuccess()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAKAAAAAAAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageDataSuccess), true);
    }

    /// <summary>
    /// Verifies that ManageDataNotSupportedYet result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageDataNotSupportedYetXdr_ReturnsManageDataNotSupportedYet()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAK/////wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageDataNotSupportedYet), false);
    }

    /// <summary>
    /// Verifies that ManageDataNameNotFound result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageDataNameNotFoundXdr_ReturnsManageDataNameNotFound()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAK/////gAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageDataNameNotFound), false);
    }

    /// <summary>
    /// Verifies that ManageDataLowReserve result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageDataLowReserveXdr_ReturnsManageDataLowReserve()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAK/////QAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageDataLowReserve), false);
    }

    /// <summary>
    /// Verifies that ManageDataInvalidName result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageDataInvalidNameXdr_ReturnsManageDataInvalidName()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAK/////AAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageDataInvalidName), false);
    }
}