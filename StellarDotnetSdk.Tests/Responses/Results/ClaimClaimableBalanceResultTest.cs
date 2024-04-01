using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class ClaimClaimableBalanceResultTest
{
    [TestMethod]
    public void TestCannotClaim()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.CLAIM_CLAIMABLE_BALANCE;

        var result = new XDR.ClaimClaimableBalanceResult();
        result.Discriminant.InnerValue = XDR.ClaimClaimableBalanceResultCode.ClaimClaimableBalanceResultCodeEnum
            .CLAIM_CLAIMABLE_BALANCE_CANNOT_CLAIM;
        operationResultTr.ClaimClaimableBalanceResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr),
            typeof(ClaimClaimableBalanceCannotClaim), false);
    }

    [TestMethod]
    public void TestDoesNotExist()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.CLAIM_CLAIMABLE_BALANCE;

        var result = new XDR.ClaimClaimableBalanceResult();
        result.Discriminant.InnerValue = XDR.ClaimClaimableBalanceResultCode.ClaimClaimableBalanceResultCodeEnum
            .CLAIM_CLAIMABLE_BALANCE_DOES_NOT_EXIST;
        operationResultTr.ClaimClaimableBalanceResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr),
            typeof(ClaimClaimableBalanceDoesNotExist), false);
    }

    [TestMethod]
    public void TestLineFull()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.CLAIM_CLAIMABLE_BALANCE;

        var result = new XDR.ClaimClaimableBalanceResult();
        result.Discriminant.InnerValue = XDR.ClaimClaimableBalanceResultCode.ClaimClaimableBalanceResultCodeEnum
            .CLAIM_CLAIMABLE_BALANCE_LINE_FULL;
        operationResultTr.ClaimClaimableBalanceResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr),
            typeof(ClaimClaimableBalanceLineFull), false);
    }

    [TestMethod]
    public void TestNotAuthorized()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.CLAIM_CLAIMABLE_BALANCE;

        var result = new XDR.ClaimClaimableBalanceResult();
        result.Discriminant.InnerValue = XDR.ClaimClaimableBalanceResultCode.ClaimClaimableBalanceResultCodeEnum
            .CLAIM_CLAIMABLE_BALANCE_NOT_AUTHORIZED;
        operationResultTr.ClaimClaimableBalanceResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr),
            typeof(ClaimClaimableBalanceNotAuthorized), false);
    }

    [TestMethod]
    public void TestNoTrust()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.CLAIM_CLAIMABLE_BALANCE;

        var result = new XDR.ClaimClaimableBalanceResult();
        result.Discriminant.InnerValue = XDR.ClaimClaimableBalanceResultCode.ClaimClaimableBalanceResultCodeEnum
            .CLAIM_CLAIMABLE_BALANCE_NO_TRUST;
        operationResultTr.ClaimClaimableBalanceResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr),
            typeof(ClaimClaimableBalanceNoTrust), false);
    }

    [TestMethod]
    public void TestSuccess()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.CLAIM_CLAIMABLE_BALANCE;

        var result = new XDR.ClaimClaimableBalanceResult();
        result.Discriminant.InnerValue = XDR.ClaimClaimableBalanceResultCode.ClaimClaimableBalanceResultCodeEnum
            .CLAIM_CLAIMABLE_BALANCE_SUCCESS;
        operationResultTr.ClaimClaimableBalanceResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr),
            typeof(ClaimClaimableBalanceSuccess), true);
    }
}