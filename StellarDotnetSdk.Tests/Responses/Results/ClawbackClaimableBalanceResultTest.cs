using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Xdr;
using ClawbackClaimableBalanceResult = StellarDotnetSdk.Xdr.ClawbackClaimableBalanceResult;
using OperationResult = StellarDotnetSdk.Xdr.OperationResult;
using ResultCodeEnum = StellarDotnetSdk.Xdr.ClawbackClaimableBalanceResultCode.ClawbackClaimableBalanceResultCodeEnum;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
///     Unit tests for clawback claimable balance result types.
/// </summary>
[TestClass]
public class ClawbackClaimableBalanceResultTest
{
    /// <summary>
    ///     Verifies that ClawbackClaimableBalanceDoesNotExist result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithClawbackClaimableBalanceDoesNotExistXdr_ReturnsClawbackClaimableBalanceDoesNotExist()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAWBACK_CLAIMABLE_BALANCE_DOES_NOT_EXIST);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ClawbackClaimableBalanceDoesNotExist), false);
    }

    /// <summary>
    ///     Verifies that ClawbackClaimableBalanceNotClawbackEnabled result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void
        Deserialize_WithClawbackClaimableBalanceNotClawbackEnabledXdr_ReturnsClawbackClaimableBalanceNotClawbackEnabled()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAWBACK_CLAIMABLE_BALANCE_NOT_CLAWBACK_ENABLED);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ClawbackClaimableBalanceNotClawbackEnabled), false);
    }

    /// <summary>
    ///     Verifies that ClawbackClaimableBalanceNotIssuer result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithClawbackClaimableBalanceNotIssuerXdr_ReturnsClawbackClaimableBalanceNotIssuer()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAWBACK_CLAIMABLE_BALANCE_NOT_ISSUER);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ClawbackClaimableBalanceNotIssuer), false);
    }

    /// <summary>
    ///     Verifies that ClawbackClaimableBalanceSuccess result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithClawbackClaimableBalanceSuccessXdr_ReturnsClawbackClaimableBalanceSuccess()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAWBACK_CLAIMABLE_BALANCE_SUCCESS);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ClawbackClaimableBalanceSuccess), true);
    }

    private static OperationResult.OperationResultTr CreateOperationResultTr(ResultCodeEnum type)
    {
        return new OperationResult.OperationResultTr
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CLAWBACK_CLAIMABLE_BALANCE),
            ClawbackClaimableBalanceResult = new ClawbackClaimableBalanceResult
            {
                Discriminant = ClawbackClaimableBalanceResultCode.Create(type),
            },
        };
    }
}