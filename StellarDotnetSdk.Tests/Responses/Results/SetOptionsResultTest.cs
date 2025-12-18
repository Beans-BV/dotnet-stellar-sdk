using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
/// Unit tests for set options result types.
/// </summary>
[TestClass]
public class SetOptionsResultTest
{
    /// <summary>
    /// Verifies that SetOptionsSuccess result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSetOptionsSuccessXdr_ReturnsSetOptionsSuccess()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAFAAAAAAAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(SetOptionsSuccess), true);
    }

    /// <summary>
    /// Verifies that SetOptionsLowReserve result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSetOptionsLowReserveXdr_ReturnsSetOptionsLowReserve()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAF/////wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(SetOptionsLowReserve), false);
    }

    /// <summary>
    /// Verifies that SetOptionsTooManySigners result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSetOptionsTooManySignersXdr_ReturnsSetOptionsTooManySigners()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAF/////gAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(SetOptionsTooManySigners), false);
    }

    /// <summary>
    /// Verifies that SetOptionsBadFlags result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSetOptionsBadFlagsXdr_ReturnsSetOptionsBadFlags()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAF/////QAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(SetOptionsBadFlags), false);
    }

    /// <summary>
    /// Verifies that SetOptionsInvalidInflation result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSetOptionsInvalidInflationXdr_ReturnsSetOptionsInvalidInflation()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAF/////AAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(SetOptionsInvalidInflation), false);
    }

    /// <summary>
    /// Verifies that SetOptionsCantChange result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSetOptionsCantChangeXdr_ReturnsSetOptionsCantChange()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAF////+wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(SetOptionsCantChange), false);
    }

    /// <summary>
    /// Verifies that SetOptionsUnknownFlag result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSetOptionsUnknownFlagXdr_ReturnsSetOptionsUnknownFlag()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAF////+gAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(SetOptionsUnknownFlag), false);
    }

    /// <summary>
    /// Verifies that SetOptionsThresholdOutOfRange result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSetOptionsThresholdOutOfRangeXdr_ReturnsSetOptionsThresholdOutOfRange()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAF////+QAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(SetOptionsThresholdOutOfRange), false);
    }

    /// <summary>
    /// Verifies that SetOptionsBadSigner result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSetOptionsBadSignerXdr_ReturnsSetOptionsBadSigner()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAF////+AAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(SetOptionsBadSigner), false);
    }

    /// <summary>
    /// Verifies that SetOptionsInvalidHomeDomain result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSetOptionsInvalidHomeDomainXdr_ReturnsSetOptionsInvalidHomeDomain()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAF////9wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(SetOptionsInvalidHomeDomain), false);
    }
}