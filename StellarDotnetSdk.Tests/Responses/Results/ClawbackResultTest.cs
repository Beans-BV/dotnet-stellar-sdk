using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Xdr;
using ClawbackResult = StellarDotnetSdk.Xdr.ClawbackResult;
using OperationResult = StellarDotnetSdk.Xdr.OperationResult;
using ResultCodeEnum = StellarDotnetSdk.Xdr.ClawbackResultCode.ClawbackResultCodeEnum;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
/// Unit tests for clawback result types.
/// </summary>
[TestClass]
public class ClawbackResultTest
{
    /// <summary>
    /// Verifies that ClawbackMalformed result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithClawbackMalformedXdr_ReturnsClawbackMalformed()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAWBACK_MALFORMED);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ClawbackMalformed), false);
    }

    /// <summary>
    /// Verifies that ClawbackNotClawbackEnabled result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithClawbackNotClawbackEnabledXdr_ReturnsClawbackNotClawbackEnabled()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAWBACK_NOT_CLAWBACK_ENABLED);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ClawbackNotClawbackEnabled), false);
    }

    /// <summary>
    /// Verifies that ClawbackNoTrust result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithClawbackNoTrustXdr_ReturnsClawbackNoTrust()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAWBACK_NO_TRUST);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ClawbackNoTrust), false);
    }

    /// <summary>
    /// Verifies that ClawbackSuccess result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithClawbackSuccessXdr_ReturnsClawbackSuccess()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAWBACK_SUCCESS);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ClawbackSuccess), true);
    }

    /// <summary>
    /// Verifies that ClawbackUnderfunded result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithClawbackUnderfundedXdr_ReturnsClawbackUnderfunded()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAWBACK_UNDERFUNDED);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ClawbackUnderfunded), false);
    }

    private static OperationResult.OperationResultTr CreateOperationResultTr(ResultCodeEnum type)
    {
        return new OperationResult.OperationResultTr
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CLAWBACK),
            ClawbackResult = new ClawbackResult
            {
                Discriminant = ClawbackResultCode.Create(type),
            },
        };
    }
}