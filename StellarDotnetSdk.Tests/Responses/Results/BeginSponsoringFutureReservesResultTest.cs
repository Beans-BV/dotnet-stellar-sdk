using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
///     Unit tests for begin sponsoring future reserves result types.
/// </summary>
[TestClass]
public class BeginSponsoringFutureReservesResultTest
{
    /// <summary>
    ///     Verifies that BeginSponsoringFutureReservesAlreadySponsored result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void
        Deserialize_WithBeginSponsoringFutureReservesAlreadySponsoredXdr_ReturnsBeginSponsoringFutureReservesAlreadySponsored()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue =
            XDR.OperationType.OperationTypeEnum.BEGIN_SPONSORING_FUTURE_RESERVES;

        var result = new XDR.BeginSponsoringFutureReservesResult();
        result.Discriminant.InnerValue = XDR.BeginSponsoringFutureReservesResultCode
            .BeginSponsoringFutureReservesResultCodeEnum.BEGIN_SPONSORING_FUTURE_RESERVES_ALREADY_SPONSORED;
        operationResultTr.BeginSponsoringFutureReservesResult = result;
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(BeginSponsoringFutureReservesAlreadySponsored), false);
    }

    /// <summary>
    ///     Verifies that BeginSponsoringFutureReservesMalformed result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void
        Deserialize_WithBeginSponsoringFutureReservesMalformedXdr_ReturnsBeginSponsoringFutureReservesMalformed()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue =
            XDR.OperationType.OperationTypeEnum.BEGIN_SPONSORING_FUTURE_RESERVES;

        var result = new XDR.BeginSponsoringFutureReservesResult();
        result.Discriminant.InnerValue = XDR.BeginSponsoringFutureReservesResultCode
            .BeginSponsoringFutureReservesResultCodeEnum.BEGIN_SPONSORING_FUTURE_RESERVES_MALFORMED;
        operationResultTr.BeginSponsoringFutureReservesResult = result;
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(BeginSponsoringFutureReservesMalformed), false);
    }

    /// <summary>
    ///     Verifies that BeginSponsoringFutureReservesRecursive result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void
        Deserialize_WithBeginSponsoringFutureReservesRecursiveXdr_ReturnsBeginSponsoringFutureReservesRecursive()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue =
            XDR.OperationType.OperationTypeEnum.BEGIN_SPONSORING_FUTURE_RESERVES;

        var result = new XDR.BeginSponsoringFutureReservesResult();
        result.Discriminant.InnerValue = XDR.BeginSponsoringFutureReservesResultCode
            .BeginSponsoringFutureReservesResultCodeEnum.BEGIN_SPONSORING_FUTURE_RESERVES_RECURSIVE;
        operationResultTr.BeginSponsoringFutureReservesResult = result;
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(BeginSponsoringFutureReservesRecursive), false);
    }

    /// <summary>
    ///     Verifies that BeginSponsoringFutureReservesSuccess result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithBeginSponsoringFutureReservesSuccessXdr_ReturnsBeginSponsoringFutureReservesSuccess()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr();
        operationResultTr.Discriminant.InnerValue =
            XDR.OperationType.OperationTypeEnum.BEGIN_SPONSORING_FUTURE_RESERVES;

        var result = new XDR.BeginSponsoringFutureReservesResult();
        result.Discriminant.InnerValue = XDR.BeginSponsoringFutureReservesResultCode
            .BeginSponsoringFutureReservesResultCodeEnum.BEGIN_SPONSORING_FUTURE_RESERVES_SUCCESS;
        operationResultTr.BeginSponsoringFutureReservesResult = result;
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(BeginSponsoringFutureReservesSuccess), true);
    }
}