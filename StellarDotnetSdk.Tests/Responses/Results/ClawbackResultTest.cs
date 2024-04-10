using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Xdr;
using ClawbackResult = StellarDotnetSdk.Xdr.ClawbackResult;
using OperationResult = StellarDotnetSdk.Xdr.OperationResult;
using ResultCodeEnum = StellarDotnetSdk.Xdr.ClawbackResultCode.ClawbackResultCodeEnum;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class ClawbackResultTest
{
    [TestMethod]
    public void TestMalformed()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAWBACK_MALFORMED);

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr), typeof(ClawbackMalformed),
            false);
    }

    [TestMethod]
    public void TestNotClawbackEnabled()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAWBACK_NOT_CLAWBACK_ENABLED);

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(ClawbackNotClawbackEnabled),
            false);
    }

    [TestMethod]
    public void TestNoTrust()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAWBACK_NO_TRUST);

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr), typeof(ClawbackNoTrust),
            false);
    }

    [TestMethod]
    public void TestSuccess()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAWBACK_SUCCESS);

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr), typeof(ClawbackSuccess),
            true);
    }

    [TestMethod]
    public void TestUnderfunded()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAWBACK_UNDERFUNDED);

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr), typeof(ClawbackUnderfunded),
            false);
    }

    private static OperationResult.OperationResultTr CreateOperationResultTr(ResultCodeEnum type)
    {
        return new OperationResult.OperationResultTr
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CLAWBACK),
            ClawbackResult = new ClawbackResult
            {
                Discriminant = ClawbackResultCode.Create(type)
            }
        };
    }
}