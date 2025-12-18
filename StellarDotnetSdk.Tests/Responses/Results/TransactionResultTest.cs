using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
/// Unit tests for <see cref="TransactionResult"/> class.
/// </summary>
[TestClass]
public class TransactionResultTest
{
    /// <summary>
    /// Verifies that TransactionResultTooEarly can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithTooEarlyXdr_ReturnsTransactionResultTooEarly()
    {
        // Arrange
        var xdrBase64 = "AAAAAAAPQkD////+AAAAAA==";

        // Act
        var result = TransactionResult.FromXdrBase64(xdrBase64);

        // Assert
        Assert.AreEqual("0.1", result.FeeCharged);
        Assert.IsInstanceOfType(result, typeof(TransactionResultTooEarly));
    }

    /// <summary>
    /// Verifies that TransactionResultTooLate can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithTooLateXdr_ReturnsTransactionResultTooLate()
    {
        // Arrange
        var xdrBase64 = "AAAAAAAPQkD////9AAAAAA==";

        // Act
        var result = TransactionResult.FromXdrBase64(xdrBase64);

        // Assert
        Assert.AreEqual("0.1", result.FeeCharged);
        Assert.IsInstanceOfType(result, typeof(TransactionResultTooLate));
    }

    /// <summary>
    /// Verifies that TransactionResultMissingOperation can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithMissingOperationXdr_ReturnsTransactionResultMissingOperation()
    {
        // Arrange
        var xdrBase64 = " AAAAAAAPQkD////8AAAAAA==";

        // Act
        var result = TransactionResult.FromXdrBase64(xdrBase64);

        // Assert
        Assert.AreEqual("0.1", result.FeeCharged);
        Assert.IsInstanceOfType(result, typeof(TransactionResultMissingOperation));
    }

    /// <summary>
    /// Verifies that TransactionResultBadSeq can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithBadSeqXdr_ReturnsTransactionResultBadSeq()
    {
        // Arrange
        var xdrBase64 = "AAAAAAAPQkD////7AAAAAA==";

        // Act
        var result = TransactionResult.FromXdrBase64(xdrBase64);

        // Assert
        Assert.AreEqual("0.1", result.FeeCharged);
        Assert.IsInstanceOfType(result, typeof(TransactionResultBadSeq));
    }

    /// <summary>
    /// Verifies that TransactionResultBadAuth can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithBadAuthXdr_ReturnsTransactionResultBadAuth()
    {
        // Arrange
        var xdrBase64 = "AAAAAAAPQkD////6AAAAAA==";

        // Act
        var result = TransactionResult.FromXdrBase64(xdrBase64);

        // Assert
        Assert.AreEqual("0.1", result.FeeCharged);
        Assert.IsInstanceOfType(result, typeof(TransactionResultBadAuth));
    }

    /// <summary>
    /// Verifies that TransactionResultInsufficientBalance can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithInsufficientBalanceXdr_ReturnsTransactionResultInsufficientBalance()
    {
        // Arrange
        var xdrBase64 = "AAAAAAAPQkD////5AAAAAA==";

        // Act
        var result = TransactionResult.FromXdrBase64(xdrBase64);

        // Assert
        Assert.AreEqual("0.1", result.FeeCharged);
        Assert.IsInstanceOfType(result, typeof(TransactionResultInsufficientBalance));
    }

    /// <summary>
    /// Verifies that TransactionResultNoAccount can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithNoAccountXdr_ReturnsTransactionResultNoAccount()
    {
        // Arrange
        var xdrBase64 = "AAAAAAAPQkD////4AAAAAA==";

        // Act
        var result = TransactionResult.FromXdrBase64(xdrBase64);

        // Assert
        Assert.AreEqual("0.1", result.FeeCharged);
        Assert.IsInstanceOfType(result, typeof(TransactionResultNoAccount));
    }

    /// <summary>
    /// Verifies that TransactionResultInsufficientFee can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithInsufficientFeeXdr_ReturnsTransactionResultInsufficientFee()
    {
        // Arrange
        var xdrBase64 = "AAAAAAAPQkD////3AAAAAA==";

        // Act
        var result = TransactionResult.FromXdrBase64(xdrBase64);

        // Assert
        Assert.AreEqual("0.1", result.FeeCharged);
        Assert.IsInstanceOfType(result, typeof(TransactionResultInsufficientFee));
    }

    /// <summary>
    /// Verifies that TransactionResultBadAuthExtra can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithBadAuthExtraXdr_ReturnsTransactionResultBadAuthExtra()
    {
        // Arrange
        var xdrBase64 = "AAAAAAAPQkD////2AAAAAA==";

        // Act
        var result = TransactionResult.FromXdrBase64(xdrBase64);

        // Assert
        Assert.AreEqual("0.1", result.FeeCharged);
        Assert.IsInstanceOfType(result, typeof(TransactionResultBadAuthExtra));
    }

    /// <summary>
    /// Verifies that TransactionResultInternalError can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithInternalErrorXdr_ReturnsTransactionResultInternalError()
    {
        // Arrange
        var xdrBase64 = "AAAAAAAPQkD////1AAAAAA==";

        // Act
        var result = TransactionResult.FromXdrBase64(xdrBase64);

        // Assert
        Assert.AreEqual("0.1", result.FeeCharged);
        Assert.IsInstanceOfType(result, typeof(TransactionResultInternalError));
    }
}