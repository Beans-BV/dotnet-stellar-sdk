using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
/// Unit tests for end sponsoring future reserves result types.
/// </summary>
[TestClass]
public class EndSponsoringFutureReservesResultTest
{
    /// <summary>
    /// Verifies that EndSponsoringFutureReservesNotSponsored result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithEndSponsoringFutureReservesNotSponsoredXdr_ReturnsEndSponsoringFutureReservesNotSponsored()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.END_SPONSORING_FUTURE_RESERVES;

        var result = new XDR.EndSponsoringFutureReservesResult();
        result.Discriminant.InnerValue = XDR.EndSponsoringFutureReservesResultCode
            .EndSponsoringFutureReservesResultCodeEnum.END_SPONSORING_FUTURE_RESERVES_NOT_SPONSORED;
        operationResultTr.EndSponsoringFutureReservesResult = result;
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(EndSponsoringFutureReservesNotSponsored), false);
    }

    /// <summary>
    /// Verifies that EndSponsoringFutureReservesSuccess result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithEndSponsoringFutureReservesSuccessXdr_ReturnsEndSponsoringFutureReservesSuccess()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue = XDR.OperationType.OperationTypeEnum.END_SPONSORING_FUTURE_RESERVES;

        var result = new XDR.EndSponsoringFutureReservesResult();
        result.Discriminant.InnerValue = XDR.EndSponsoringFutureReservesResultCode
            .EndSponsoringFutureReservesResultCodeEnum.END_SPONSORING_FUTURE_RESERVES_SUCCESS;
        operationResultTr.EndSponsoringFutureReservesResult = result;
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(EndSponsoringFutureReservesSuccess), true);
    }
}