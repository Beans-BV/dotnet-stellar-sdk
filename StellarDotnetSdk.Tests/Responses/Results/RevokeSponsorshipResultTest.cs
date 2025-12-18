using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
///     Unit tests for revoke sponsorship result types.
/// </summary>
[TestClass]
public class RevokeSponsorshipResultTest
{
    /// <summary>
    ///     Verifies that RevokeSponsorshipDoesNotExist result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithRevokeSponsorshipDoesNotExistXdr_ReturnsRevokeSponsorshipDoesNotExist()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.REVOKE_SPONSORSHIP,
            },
        };

        var result = new XDR.RevokeSponsorshipResult
        {
            Discriminant =
            {
                InnerValue = XDR.RevokeSponsorshipResultCode.RevokeSponsorshipResultCodeEnum
                    .REVOKE_SPONSORSHIP_DOES_NOT_EXIST,
            },
        };
        operationResultTr.RevokeSponsorshipResult = result;
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(RevokeSponsorshipDoesNotExist), false);
    }

    /// <summary>
    ///     Verifies that RevokeSponsorshipLowReserve result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithRevokeSponsorshipLowReserveXdr_ReturnsRevokeSponsorshipLowReserve()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.REVOKE_SPONSORSHIP,
            },
        };

        var result = new XDR.RevokeSponsorshipResult
        {
            Discriminant =
            {
                InnerValue = XDR.RevokeSponsorshipResultCode.RevokeSponsorshipResultCodeEnum
                    .REVOKE_SPONSORSHIP_LOW_RESERVE,
            },
        };
        operationResultTr.RevokeSponsorshipResult = result;
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(RevokeSponsorshipLowReserve), false);
    }

    /// <summary>
    ///     Verifies that RevokeSponsorshipNotSponsor result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithRevokeSponsorshipNotSponsorXdr_ReturnsRevokeSponsorshipNotSponsor()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.REVOKE_SPONSORSHIP,
            },
        };

        var result = new XDR.RevokeSponsorshipResult
        {
            Discriminant =
            {
                InnerValue = XDR.RevokeSponsorshipResultCode.RevokeSponsorshipResultCodeEnum
                    .REVOKE_SPONSORSHIP_NOT_SPONSOR,
            },
        };
        operationResultTr.RevokeSponsorshipResult = result;
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(RevokeSponsorshipNotSponsor), false);
    }

    /// <summary>
    ///     Verifies that RevokeSponsorshipOnlyTransferable result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithRevokeSponsorshipOnlyTransferableXdr_ReturnsRevokeSponsorshipOnlyTransferable()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.REVOKE_SPONSORSHIP,
            },
        };

        var result = new XDR.RevokeSponsorshipResult
        {
            Discriminant =
            {
                InnerValue = XDR.RevokeSponsorshipResultCode.RevokeSponsorshipResultCodeEnum
                    .REVOKE_SPONSORSHIP_ONLY_TRANSFERABLE,
            },
        };
        operationResultTr.RevokeSponsorshipResult = result;
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(RevokeSponsorshipOnlyTransferable), false);
    }

    /// <summary>
    ///     Verifies that RevokeSponsorshipSuccess result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithRevokeSponsorshipSuccessXdr_ReturnsRevokeSponsorshipSuccess()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.REVOKE_SPONSORSHIP,
            },
        };

        var result = new XDR.RevokeSponsorshipResult
        {
            Discriminant =
            {
                InnerValue = XDR.RevokeSponsorshipResultCode.RevokeSponsorshipResultCodeEnum.REVOKE_SPONSORSHIP_SUCCESS,
            },
        };
        operationResultTr.RevokeSponsorshipResult = result;
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(RevokeSponsorshipSuccess), true);
    }
}