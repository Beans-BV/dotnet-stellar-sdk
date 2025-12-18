using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
/// Unit tests for account merge result types.
/// </summary>
[TestClass]
public class AccountMergeResultTest
{
    /// <summary>
    /// Verifies that AccountMergeSuccess result can be deserialized correctly and contains source account balance.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAccountMergeSuccessXdr_ReturnsAccountMergeSuccessWithBalance()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAIAAAAAAAAAAAF9eEAAAAAAA==";

        // Act
        var tx = Utils.AssertResultOfType(xdrBase64, typeof(AccountMergeSuccess), true);

        // Assert
        var failed = (TransactionResultFailed)tx;
        var op = (AccountMergeSuccess)failed.Results[0];
        Assert.AreEqual("10", op.SourceAccountBalance);
    }

    /// <summary>
    /// Verifies that AccountMergeMalformed result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAccountMergeMalformedXdr_ReturnsAccountMergeMalformed()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAI/////wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(AccountMergeMalformed), false);
    }

    /// <summary>
    /// Verifies that AccountMergeNoAccount result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAccountMergeNoAccountXdr_ReturnsAccountMergeNoAccount()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAI/////gAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(AccountMergeNoAccount), false);
    }

    /// <summary>
    /// Verifies that AccountMergeImmutableSet result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAccountMergeImmutableSetXdr_ReturnsAccountMergeImmutableSet()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAI/////QAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(AccountMergeImmutableSet), false);
    }

    /// <summary>
    /// Verifies that AccountMergeHasSubEntries result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAccountMergeHasSubEntriesXdr_ReturnsAccountMergeHasSubEntries()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAI/////AAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(AccountMergeHasSubEntries), false);
    }

    /// <summary>
    /// Verifies that AccountMergeSequenceNumberTooFar result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAccountMergeSequenceNumberTooFarXdr_ReturnsAccountMergeSequenceNumberTooFar()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAI////+wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(AccountMergeSequenceNumberTooFar), false);
    }

    /// <summary>
    /// Verifies that AccountMergeDestFull result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAccountMergeDestFullXdr_ReturnsAccountMergeDestFull()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAI////+gAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(AccountMergeDestFull), false);
    }
}