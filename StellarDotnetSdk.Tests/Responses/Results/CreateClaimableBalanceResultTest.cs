using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Xdr;
using CreateClaimableBalanceResult = StellarDotnetSdk.Xdr.CreateClaimableBalanceResult;
using OperationResult = StellarDotnetSdk.Xdr.OperationResult;
using ResultCodeEnum = StellarDotnetSdk.Xdr.CreateClaimableBalanceResultCode.CreateClaimableBalanceResultCodeEnum;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class CreateClaimableBalanceResultTest
{
    [TestMethod]
    public void TestLowReserve()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CREATE_CLAIMABLE_BALANCE_LOW_RESERVE);

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(CreateClaimableBalanceLowReserve), false);
    }

    [TestMethod]
    public void TestMalformed()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CREATE_CLAIMABLE_BALANCE_MALFORMED);

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(CreateClaimableBalanceMalformed), false);
    }

    [TestMethod]
    public void TestNotAuthorized()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CREATE_CLAIMABLE_BALANCE_NOT_AUTHORIZED);

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(CreateClaimableBalanceNotAuthorized), false);
    }

    [TestMethod]
    public void TestNoTrust()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CREATE_CLAIMABLE_BALANCE_NO_TRUST);

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(CreateClaimableBalanceNoTrust), false);
    }

    [TestMethod]
    public void TestSuccess()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CREATE_CLAIMABLE_BALANCE_SUCCESS);
        operationResultTr.CreateClaimableBalanceResult.BalanceID =
            new ClaimableBalanceID
            {
                Discriminant = ClaimableBalanceIDType.Create(ClaimableBalanceIDType.ClaimableBalanceIDTypeEnum
                    .CLAIMABLE_BALANCE_ID_TYPE_V0),
                V0 = new Hash(Convert.FromBase64String("i7gJhVls6QELGhMtAlC+ScMatzkwXW/s9+UoKVhN13Y="))
            };

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(CreateClaimableBalanceSuccess), true);
    }

    [TestMethod]
    public void TestUnderfunded()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CREATE_CLAIMABLE_BALANCE_UNDERFUNDED);

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(CreateClaimableBalanceUnderfunded), false);
    }

    private static OperationResult.OperationResultTr CreateOperationResultTr(ResultCodeEnum type)
    {
        return new OperationResult.OperationResultTr
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CREATE_CLAIMABLE_BALANCE),
            CreateClaimableBalanceResult = new CreateClaimableBalanceResult
            {
                Discriminant = CreateClaimableBalanceResultCode.Create(type)
            }
        };
    }
}