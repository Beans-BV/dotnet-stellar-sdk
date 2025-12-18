using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Xdr;
using CreateClaimableBalanceResult = StellarDotnetSdk.Xdr.CreateClaimableBalanceResult;
using OperationResult = StellarDotnetSdk.Xdr.OperationResult;
using ResultCodeEnum = StellarDotnetSdk.Xdr.CreateClaimableBalanceResultCode.CreateClaimableBalanceResultCodeEnum;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
///     Unit tests for create claimable balance result types.
/// </summary>
[TestClass]
public class CreateClaimableBalanceResultTest
{
    /// <summary>
    ///     Verifies that CreateClaimableBalanceLowReserve result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithCreateClaimableBalanceLowReserveXdr_ReturnsCreateClaimableBalanceLowReserve()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CREATE_CLAIMABLE_BALANCE_LOW_RESERVE);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(CreateClaimableBalanceLowReserve), false);
    }

    /// <summary>
    ///     Verifies that CreateClaimableBalanceMalformed result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithCreateClaimableBalanceMalformedXdr_ReturnsCreateClaimableBalanceMalformed()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CREATE_CLAIMABLE_BALANCE_MALFORMED);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(CreateClaimableBalanceMalformed), false);
    }

    /// <summary>
    ///     Verifies that CreateClaimableBalanceNotAuthorized result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithCreateClaimableBalanceNotAuthorizedXdr_ReturnsCreateClaimableBalanceNotAuthorized()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CREATE_CLAIMABLE_BALANCE_NOT_AUTHORIZED);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(CreateClaimableBalanceNotAuthorized), false);
    }

    /// <summary>
    ///     Verifies that CreateClaimableBalanceNoTrust result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithCreateClaimableBalanceNoTrustXdr_ReturnsCreateClaimableBalanceNoTrust()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CREATE_CLAIMABLE_BALANCE_NO_TRUST);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(CreateClaimableBalanceNoTrust), false);
    }

    /// <summary>
    ///     Verifies that CreateClaimableBalanceSuccess result can be deserialized correctly and contains balance ID.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithCreateClaimableBalanceSuccessXdr_ReturnsCreateClaimableBalanceSuccessWithBalanceId()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CREATE_CLAIMABLE_BALANCE_SUCCESS);
        operationResultTr.CreateClaimableBalanceResult.BalanceID =
            new ClaimableBalanceID
            {
                Discriminant = ClaimableBalanceIDType.Create(ClaimableBalanceIDType.ClaimableBalanceIDTypeEnum
                    .CLAIMABLE_BALANCE_ID_TYPE_V0),
                V0 = new Hash(Convert.FromBase64String("i7gJhVls6QELGhMtAlC+ScMatzkwXW/s9+UoKVhN13Y=")),
            };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(CreateClaimableBalanceSuccess), true);
    }

    /// <summary>
    ///     Verifies that CreateClaimableBalanceUnderfunded result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithCreateClaimableBalanceUnderfundedXdr_ReturnsCreateClaimableBalanceUnderfunded()
    {
        // Arrange
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CREATE_CLAIMABLE_BALANCE_UNDERFUNDED);
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(CreateClaimableBalanceUnderfunded), false);
    }

    private static OperationResult.OperationResultTr CreateOperationResultTr(ResultCodeEnum type)
    {
        return new OperationResult.OperationResultTr
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CREATE_CLAIMABLE_BALANCE),
            CreateClaimableBalanceResult = new CreateClaimableBalanceResult
            {
                Discriminant = CreateClaimableBalanceResultCode.Create(type),
            },
        };
    }
}