using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class ClawbackResultTest
{
    [TestMethod]
    public void TestMalformed()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.CLAWBACK;

        var result = new XDR.ClawbackResult();
        result.Discriminant.InnerValue = XDR.ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_MALFORMED;
        operationResultTr.ClawbackResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr), typeof(ClawbackMalformed), false);
    }

    [TestMethod]
    public void TestNotClawbackEnabled()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.CLAWBACK;

        var result = new XDR.ClawbackResult();
        result.Discriminant.InnerValue = XDR.ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_NOT_CLAWBACK_ENABLED;
        operationResultTr.ClawbackResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr), typeof(ClawbackNotClawbackEnabled),
            false);
    }

    [TestMethod]
    public void TestNoTrust()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.CLAWBACK;

        var result = new XDR.ClawbackResult();
        result.Discriminant.InnerValue = XDR.ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_NO_TRUST;
        operationResultTr.ClawbackResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr), typeof(ClawbackNoTrust), false);
    }

    [TestMethod]
    public void TestSuccess()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.CLAWBACK;

        var result = new XDR.ClawbackResult();
        result.Discriminant.InnerValue = XDR.ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_SUCCESS;
        operationResultTr.ClawbackResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr), typeof(ClawbackSuccess), true);
    }

    [TestMethod]
    public void TestUnderfunded()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.CLAWBACK;

        var result = new XDR.ClawbackResult();
        result.Discriminant.InnerValue = XDR.ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_UNDERFUNDED;
        operationResultTr.ClawbackResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr), typeof(ClawbackUnderfunded), false);
    }
}