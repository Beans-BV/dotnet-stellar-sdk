using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
///     Unit tests for create account result types.
/// </summary>
[TestClass]
public class CreateAccountResultTest
{
    /// <summary>
    ///     Verifies that CreateAccountSuccess result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithCreateAccountSuccessXdr_ReturnsCreateAccountSuccess()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAAAAAAAAAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(CreateAccountSuccess), true);
    }

    /// <summary>
    ///     Verifies that CreateAccountMalformed result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithCreateAccountMalformedXdr_ReturnsCreateAccountMalformed()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAA/////wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(CreateAccountMalformed), false);
    }

    /// <summary>
    ///     Verifies that CreateAccountUnderfunded result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithCreateAccountUnderfundedXdr_ReturnsCreateAccountUnderfunded()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAA/////gAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(CreateAccountUnderfunded), false);
    }

    /// <summary>
    ///     Verifies that CreateAccountLowReserve result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithCreateAccountLowReserveXdr_ReturnsCreateAccountLowReserve()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAA/////QAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(CreateAccountLowReserve), false);
    }

    /// <summary>
    ///     Verifies that CreateAccountAlreadyExists result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithCreateAccountAlreadyExistsXdr_ReturnsCreateAccountAlreadyExists()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAA/////AAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(CreateAccountAlreadyExists), false);
    }
}