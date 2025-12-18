using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
///     Unit tests for operation result types.
/// </summary>
[TestClass]
public class OperationResultTest
{
    /// <summary>
    ///     Verifies that OperationResultBadAuth can be deserialized from a failed transaction result.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithBadAuthOperationXdr_ReturnsOperationResultBadAuth()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAf////8AAAAA";

        // Act
        var result = TransactionResult.FromXdrBase64(xdrBase64);

        // Assert
        Assert.IsInstanceOfType(result, typeof(TransactionResultFailed));
        var failed = (TransactionResultFailed)result;
        Assert.IsFalse(failed.IsSuccess);
        Assert.AreEqual(1, failed.Results.Count);
        var op = failed.Results[0];
        Assert.IsInstanceOfType(op, typeof(OperationResultBadAuth));
    }

    /// <summary>
    ///     Verifies that OperationResultNoAccount can be deserialized from a failed transaction result.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithNoAccountOperationXdr_ReturnsOperationResultNoAccount()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAf////4AAAAA";

        // Act
        var result = TransactionResult.FromXdrBase64(xdrBase64);

        // Assert
        Assert.IsInstanceOfType(result, typeof(TransactionResultFailed));
        var failed = (TransactionResultFailed)result;
        Assert.IsFalse(failed.IsSuccess);
        Assert.AreEqual(1, failed.Results.Count);
        var op = failed.Results[0];
        Assert.IsInstanceOfType(op, typeof(OperationResultNoAccount));
    }

    /// <summary>
    ///     Verifies that OperationResultNotSupported can be deserialized from a failed transaction result.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithNotSupportedOperationXdr_ReturnsOperationResultNotSupported()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAf////0AAAAA";

        // Act
        var result = TransactionResult.FromXdrBase64(xdrBase64);

        // Assert
        Assert.IsInstanceOfType(result, typeof(TransactionResultFailed));
        var failed = (TransactionResultFailed)result;
        Assert.IsFalse(failed.IsSuccess);
        Assert.AreEqual(1, failed.Results.Count);
        var op = failed.Results[0];
        Assert.IsInstanceOfType(op, typeof(OperationResultNotSupported));
    }

    /// <summary>
    ///     Verifies that multiple operation failures can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithMultipleFailuresXdr_ReturnsTransactionResultFailedWithMultipleResults()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAA/////3//////////gAAAAA=";

        // Act
        var result = TransactionResult.FromXdrBase64(xdrBase64);

        // Assert
        Assert.IsInstanceOfType(result, typeof(TransactionResultFailed));
        var failed = (TransactionResultFailed)result;
        Assert.IsFalse(failed.IsSuccess);
        Assert.AreEqual(3, failed.Results.Count);
        Assert.IsInstanceOfType(failed.Results[0], typeof(OperationResultNotSupported));
        Assert.IsInstanceOfType(failed.Results[1], typeof(OperationResultBadAuth));
        Assert.IsInstanceOfType(failed.Results[2], typeof(OperationResultNoAccount));
    }
}