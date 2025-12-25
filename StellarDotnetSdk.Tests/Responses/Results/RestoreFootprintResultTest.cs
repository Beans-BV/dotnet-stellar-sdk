using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
///     Unit tests for restore footprint result types.
/// </summary>
[TestClass]
public class RestoreFootprintResultTest
{
    /// <summary>
    ///     Verifies that RestoreFootprintSuccess result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithRestoreFootprintSuccessXdr_ReturnsRestoreFootprintSuccess()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.RESTORE_FOOTPRINT,
            },
            RestoreFootprintResult = new XDR.RestoreFootprintResult
            {
                Discriminant =
                {
                    InnerValue = XDR.RestoreFootprintResultCode.RestoreFootprintResultCodeEnum
                        .RESTORE_FOOTPRINT_SUCCESS,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act
        var result = TransactionResult.FromXdrBase64(xdrBase64);

        // Assert
        Assert.IsInstanceOfType(result, typeof(TransactionResultFailed));
        var failed = (TransactionResultFailed)result;
        Assert.AreEqual(1, failed.Results.Count);
        var op = failed.Results[0];
        Assert.IsInstanceOfType(op, typeof(RestoreFootprintSuccess));
        Assert.IsTrue(op.IsSuccess);
    }

    /// <summary>
    ///     Verifies that RestoreFootprintMalformed result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithRestoreFootprintMalformedXdr_ReturnsRestoreFootprintMalformed()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.RESTORE_FOOTPRINT,
            },
            RestoreFootprintResult = new XDR.RestoreFootprintResult
            {
                Discriminant =
                {
                    InnerValue = XDR.RestoreFootprintResultCode.RestoreFootprintResultCodeEnum
                        .RESTORE_FOOTPRINT_MALFORMED,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(RestoreFootprintMalformed), false);
    }

    /// <summary>
    ///     Verifies that RestoreFootprintResourceLimitExceeded result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithRestoreFootprintResourceLimitExceededXdr_ReturnsRestoreFootprintResourceLimitExceeded()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.RESTORE_FOOTPRINT,
            },
            RestoreFootprintResult = new XDR.RestoreFootprintResult
            {
                Discriminant =
                {
                    InnerValue = XDR.RestoreFootprintResultCode.RestoreFootprintResultCodeEnum
                        .RESTORE_FOOTPRINT_RESOURCE_LIMIT_EXCEEDED,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(RestoreFootprintResourceLimitExceeded), false);
    }

    /// <summary>
    ///     Verifies that RestoreFootprintInsufficientRefundableFee result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void
        Deserialize_WithRestoreFootprintInsufficientRefundableFeeXdr_ReturnsRestoreFootprintInsufficientRefundableFee()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.RESTORE_FOOTPRINT,
            },
            RestoreFootprintResult = new XDR.RestoreFootprintResult
            {
                Discriminant =
                {
                    InnerValue = XDR.RestoreFootprintResultCode.RestoreFootprintResultCodeEnum
                        .RESTORE_FOOTPRINT_INSUFFICIENT_REFUNDABLE_FEE,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(RestoreFootprintInsufficientRefundableFee), false);
    }

    /// <summary>
    ///     Verifies that FromXdr throws ArgumentOutOfRangeException for unknown result code.
    /// </summary>
    [TestMethod]
    public void FromXdr_WithUnknownResultCode_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var xdrResult = new XDR.RestoreFootprintResult
        {
            Discriminant =
            {
                InnerValue = (XDR.RestoreFootprintResultCode.RestoreFootprintResultCodeEnum)999,
            },
        };

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => RestoreFootprintResult.FromXdr(xdrResult));
    }
}