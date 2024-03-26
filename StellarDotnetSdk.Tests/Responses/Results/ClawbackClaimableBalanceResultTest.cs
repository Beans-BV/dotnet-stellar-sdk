using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class ClawbackClaimableBalanceOperationResponse
{
    [TestMethod]
    public void TestDoesNotExist()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.CLAWBACK_CLAIMABLE_BALANCE;

        var result = new XDR.ClawbackClaimableBalanceResult();
        result.Discriminant.InnerValue = XDR.ClawbackClaimableBalanceResultCode.ClawbackClaimableBalanceResultCodeEnum
            .CLAWBACK_CLAIMABLE_BALANCE_DOES_NOT_EXIST;
        operationResultTr.ClawbackClaimableBalanceResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr),
            typeof(ClawbackClaimableBalanceDoesNotExist), false);
    }

    [TestMethod]
    public void TestNotClawbackEnabled()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.CLAWBACK_CLAIMABLE_BALANCE;

        var result = new XDR.ClawbackClaimableBalanceResult();
        result.Discriminant.InnerValue = XDR.ClawbackClaimableBalanceResultCode.ClawbackClaimableBalanceResultCodeEnum
            .CLAWBACK_CLAIMABLE_BALANCE_NOT_CLAWBACK_ENABLED;
        operationResultTr.ClawbackClaimableBalanceResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr),
            typeof(ClawbackClaimableBalanceNotClawbackEnabled), false);
    }

    [TestMethod]
    public void TestNotIssuer()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.CLAWBACK_CLAIMABLE_BALANCE;

        var result = new XDR.ClawbackClaimableBalanceResult();
        result.Discriminant.InnerValue = XDR.ClawbackClaimableBalanceResultCode.ClawbackClaimableBalanceResultCodeEnum
            .CLAWBACK_CLAIMABLE_BALANCE_NOT_ISSUER;
        operationResultTr.ClawbackClaimableBalanceResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr),
            typeof(ClawbackClaimableBalanceNotIssuer), false);
    }

    [TestMethod]
    public void TestSuccess()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.CLAWBACK_CLAIMABLE_BALANCE;

        var result = new XDR.ClawbackClaimableBalanceResult();
        result.Discriminant.InnerValue = XDR.ClawbackClaimableBalanceResultCode.ClawbackClaimableBalanceResultCodeEnum
            .CLAWBACK_CLAIMABLE_BALANCE_SUCCESS;
        operationResultTr.ClawbackClaimableBalanceResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr),
            typeof(ClawbackClaimableBalanceSuccess), true);
    }
}