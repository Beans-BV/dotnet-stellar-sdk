using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class BeginSponsoringFutureReservesResultTest
{
    [TestMethod]
    public void TestAlreadySponsored()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue =
            XDR.OperationType.OperationTypeEnum.BEGIN_SPONSORING_FUTURE_RESERVES;

        var result = new XDR.BeginSponsoringFutureReservesResult();
        result.Discriminant.InnerValue = XDR.BeginSponsoringFutureReservesResultCode
            .BeginSponsoringFutureReservesResultCodeEnum.BEGIN_SPONSORING_FUTURE_RESERVES_ALREADY_SPONSORED;
        operationResultTr.BeginSponsoringFutureReservesResult = result;

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(BeginSponsoringFutureReservesAlreadySponsored), false);
    }

    [TestMethod]
    public void TestMalformed()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue =
            XDR.OperationType.OperationTypeEnum.BEGIN_SPONSORING_FUTURE_RESERVES;

        var result = new XDR.BeginSponsoringFutureReservesResult();
        result.Discriminant.InnerValue = XDR.BeginSponsoringFutureReservesResultCode
            .BeginSponsoringFutureReservesResultCodeEnum.BEGIN_SPONSORING_FUTURE_RESERVES_MALFORMED;
        operationResultTr.BeginSponsoringFutureReservesResult = result;

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(BeginSponsoringFutureReservesMalformed), false);
    }

    [TestMethod]
    public void TestRecursive()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue =
            XDR.OperationType.OperationTypeEnum.BEGIN_SPONSORING_FUTURE_RESERVES;

        var result = new XDR.BeginSponsoringFutureReservesResult();
        result.Discriminant.InnerValue = XDR.BeginSponsoringFutureReservesResultCode
            .BeginSponsoringFutureReservesResultCodeEnum.BEGIN_SPONSORING_FUTURE_RESERVES_RECURSIVE;
        operationResultTr.BeginSponsoringFutureReservesResult = result;

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(BeginSponsoringFutureReservesRecursive), false);
    }

    [TestMethod]
    public void TestSuccess()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue =
            XDR.OperationType.OperationTypeEnum.BEGIN_SPONSORING_FUTURE_RESERVES;

        var result = new XDR.BeginSponsoringFutureReservesResult();
        result.Discriminant.InnerValue = XDR.BeginSponsoringFutureReservesResultCode
            .BeginSponsoringFutureReservesResultCodeEnum.BEGIN_SPONSORING_FUTURE_RESERVES_SUCCESS;
        operationResultTr.BeginSponsoringFutureReservesResult = result;

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(BeginSponsoringFutureReservesSuccess), true);
    }
}