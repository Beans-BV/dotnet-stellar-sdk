﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class EndSponsoringFutureReservesResultTest
{
    [TestMethod]
    public void TestNotSponsored()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.END_SPONSORING_FUTURE_RESERVES;

        var result = new XDR.EndSponsoringFutureReservesResult();
        result.Discriminant.InnerValue = XDR.EndSponsoringFutureReservesResultCode
            .EndSponsoringFutureReservesResultCodeEnum.END_SPONSORING_FUTURE_RESERVES_NOT_SPONSORED;
        operationResultTr.EndSponsoringFutureReservesResult = result;

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(EndSponsoringFutureReservesNotSponsored), false);
    }

    [TestMethod]
    public void TestSuccess()
    {
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.END_SPONSORING_FUTURE_RESERVES;

        var result = new XDR.EndSponsoringFutureReservesResult();
        result.Discriminant.InnerValue = XDR.EndSponsoringFutureReservesResultCode
            .EndSponsoringFutureReservesResultCodeEnum.END_SPONSORING_FUTURE_RESERVES_SUCCESS;
        operationResultTr.EndSponsoringFutureReservesResult = result;

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(EndSponsoringFutureReservesSuccess), true);
    }
}