using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Xdr;
using ClawbackClaimableBalanceResult = StellarDotnetSdk.Xdr.ClawbackClaimableBalanceResult;
using OperationResult = StellarDotnetSdk.Xdr.OperationResult;
using ResultCodeEnum = StellarDotnetSdk.Xdr.ClawbackClaimableBalanceResultCode.ClawbackClaimableBalanceResultCodeEnum;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class ClawbackClaimableBalanceOperationResponse
{
    [TestMethod]
    public void TestDoesNotExist()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAWBACK_CLAIMABLE_BALANCE_DOES_NOT_EXIST);
        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(ClawbackClaimableBalanceDoesNotExist), false);
    }

    [TestMethod]
    public void TestNotClawbackEnabled()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAWBACK_CLAIMABLE_BALANCE_NOT_CLAWBACK_ENABLED);

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(ClawbackClaimableBalanceNotClawbackEnabled), false);
    }

    [TestMethod]
    public void TestNotIssuer()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAWBACK_CLAIMABLE_BALANCE_NOT_ISSUER);
        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(ClawbackClaimableBalanceNotIssuer), false);
    }

    [TestMethod]
    public void TestSuccess()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAWBACK_CLAIMABLE_BALANCE_SUCCESS);

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(ClawbackClaimableBalanceSuccess), true);
    }

    private static OperationResult.OperationResultTr CreateOperationResultTr(ResultCodeEnum type)
    {
        return new OperationResult.OperationResultTr
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CLAWBACK_CLAIMABLE_BALANCE),
            ClawbackClaimableBalanceResult = new ClawbackClaimableBalanceResult
            {
                Discriminant = ClawbackClaimableBalanceResultCode.Create(type)
            }
        };
    }
}