using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Xdr;
using ClaimClaimableBalanceResult = StellarDotnetSdk.Xdr.ClaimClaimableBalanceResult;
using OperationResult = StellarDotnetSdk.Xdr.OperationResult;
using ResultCodeEnum = StellarDotnetSdk.Xdr.ClaimClaimableBalanceResultCode.ClaimClaimableBalanceResultCodeEnum;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
/// Unit tests for claim claimable balance result types.
/// </summary>
[TestClass]
public class ClaimClaimableBalanceResultTest
{
    /// <summary>
    /// Verifies that ClaimClaimableBalanceCannotClaim result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithClaimClaimableBalanceCannotClaimXdr_ReturnsClaimClaimableBalanceCannotClaim()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_CANNOT_CLAIM);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ClaimClaimableBalanceCannotClaim), false);
    }

    /// <summary>
    /// Verifies that ClaimClaimableBalanceDoesNotExist result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithClaimClaimableBalanceDoesNotExistXdr_ReturnsClaimClaimableBalanceDoesNotExist()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_DOES_NOT_EXIST);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ClaimClaimableBalanceDoesNotExist), false);
    }

    /// <summary>
    /// Verifies that ClaimClaimableBalanceLineFull result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithClaimClaimableBalanceLineFullXdr_ReturnsClaimClaimableBalanceLineFull()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_LINE_FULL);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ClaimClaimableBalanceLineFull), false);
    }

    /// <summary>
    /// Verifies that ClaimClaimableBalanceNotAuthorized result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithClaimClaimableBalanceNotAuthorizedXdr_ReturnsClaimClaimableBalanceNotAuthorized()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_NOT_AUTHORIZED);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ClaimClaimableBalanceNotAuthorized), false);
    }

    /// <summary>
    /// Verifies that ClaimClaimableBalanceNoTrust result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithClaimClaimableBalanceNoTrustXdr_ReturnsClaimClaimableBalanceNoTrust()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_NO_TRUST);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ClaimClaimableBalanceNoTrust), false);
    }

    /// <summary>
    /// Verifies that ClaimClaimableBalanceSuccess result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithClaimClaimableBalanceSuccessXdr_ReturnsClaimClaimableBalanceSuccess()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_SUCCESS);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ClaimClaimableBalanceSuccess), true);
    }

    private static OperationResult.OperationResultTr CreateOperationResultTr(ResultCodeEnum type)
    {
        return new OperationResult.OperationResultTr
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CLAIM_CLAIMABLE_BALANCE),
            ClaimClaimableBalanceResult = new ClaimClaimableBalanceResult
            {
                Discriminant = ClaimClaimableBalanceResultCode.Create(type),
            },
        };
    }
}