using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class SetTrustlineFlagsResultTest
{
    [TestMethod]
    public void TestCantRevoke()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.SET_TRUST_LINE_FLAGS;

        var result = new XDR.SetTrustLineFlagsResult();
        result.Discriminant.InnerValue = XDR.SetTrustLineFlagsResultCode.SetTrustLineFlagsResultCodeEnum
            .SET_TRUST_LINE_FLAGS_CANT_REVOKE;
        operationResultTr.SetTrustLineFlagsResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr), typeof(SetTrustlineFlagsCantRevoke),
            false);
    }

    [TestMethod]
    public void TestInvalidState()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.SET_TRUST_LINE_FLAGS;

        var result = new XDR.SetTrustLineFlagsResult();
        result.Discriminant.InnerValue = XDR.SetTrustLineFlagsResultCode.SetTrustLineFlagsResultCodeEnum
            .SET_TRUST_LINE_FLAGS_INVALID_STATE;
        operationResultTr.SetTrustLineFlagsResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr),
            typeof(SetTrustlineFlagsInvalidState), false);
    }

    [TestMethod]
    public void TestMalformed()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.SET_TRUST_LINE_FLAGS;

        var result = new XDR.SetTrustLineFlagsResult();
        result.Discriminant.InnerValue = XDR.SetTrustLineFlagsResultCode.SetTrustLineFlagsResultCodeEnum
            .SET_TRUST_LINE_FLAGS_MALFORMED;
        operationResultTr.SetTrustLineFlagsResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr), typeof(SetTrustlineFlagsMalformed),
            false);
    }

    [TestMethod]
    public void TestNoTrustline()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.SET_TRUST_LINE_FLAGS;

        var result = new XDR.SetTrustLineFlagsResult();
        result.Discriminant.InnerValue = XDR.SetTrustLineFlagsResultCode.SetTrustLineFlagsResultCodeEnum
            .SET_TRUST_LINE_FLAGS_NO_TRUST_LINE;
        operationResultTr.SetTrustLineFlagsResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr),
            typeof(SetTrustlineFlagsNoTrustline), false);
    }

    [TestMethod]
    public void TestSuccess()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.SET_TRUST_LINE_FLAGS;

        var result = new XDR.SetTrustLineFlagsResult();
        result.Discriminant.InnerValue = XDR.SetTrustLineFlagsResultCode.SetTrustLineFlagsResultCodeEnum
            .SET_TRUST_LINE_FLAGS_SUCCESS;
        operationResultTr.SetTrustLineFlagsResult = result;

        Util.AssertResultOfType(Util.CreateTransactionResultXdr(operationResultTr), typeof(SetTrustlineFlagsSuccess),
            true);
    }
}