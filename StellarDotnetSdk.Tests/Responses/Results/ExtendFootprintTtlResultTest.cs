using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
///     Unit tests for extend footprint TTL result types.
/// </summary>
[TestClass]
public class ExtendFootprintTtlResultTest
{
    /// <summary>
    ///     Verifies that ExtendFootprintTtlSuccess result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithExtendFootprintTtlSuccessXdr_ReturnsExtendFootprintTtlSuccess()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.EXTEND_FOOTPRINT_TTL,
            },
            ExtendFootprintTTLResult = new XDR.ExtendFootprintTTLResult
            {
                Discriminant =
                {
                    InnerValue = XDR.ExtendFootprintTTLResultCode.ExtendFootprintTTLResultCodeEnum
                        .EXTEND_FOOTPRINT_TTL_SUCCESS,
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
        Assert.IsInstanceOfType(op, typeof(ExtendFootprintTtlSuccess));
        Assert.IsTrue(op.IsSuccess);
    }

    /// <summary>
    ///     Verifies that ExtendFootprintTtlMalformed result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithExtendFootprintTtlMalformedXdr_ReturnsExtendFootprintTtlMalformed()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.EXTEND_FOOTPRINT_TTL,
            },
            ExtendFootprintTTLResult = new XDR.ExtendFootprintTTLResult
            {
                Discriminant =
                {
                    InnerValue = XDR.ExtendFootprintTTLResultCode.ExtendFootprintTTLResultCodeEnum
                        .EXTEND_FOOTPRINT_TTL_MALFORMED,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ExtendFootprintTtlMalformed), false);
    }

    /// <summary>
    ///     Verifies that ExtendFootprintTtlResourceLimitExceeded result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithExtendFootprintTtlResourceLimitExceededXdr_ReturnsExtendFootprintTtlResourceLimitExceeded()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.EXTEND_FOOTPRINT_TTL,
            },
            ExtendFootprintTTLResult = new XDR.ExtendFootprintTTLResult
            {
                Discriminant =
                {
                    InnerValue = XDR.ExtendFootprintTTLResultCode.ExtendFootprintTTLResultCodeEnum
                        .EXTEND_FOOTPRINT_TTL_RESOURCE_LIMIT_EXCEEDED,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ExtendFootprintTtlResourceLimitExceeded), false);
    }

    /// <summary>
    ///     Verifies that ExtendFootprintTtlInsufficientRefundableFee result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithExtendFootprintTtlInsufficientRefundableFeeXdr_ReturnsExtendFootprintTtlInsufficientRefundableFee()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.EXTEND_FOOTPRINT_TTL,
            },
            ExtendFootprintTTLResult = new XDR.ExtendFootprintTTLResult
            {
                Discriminant =
                {
                    InnerValue = XDR.ExtendFootprintTTLResultCode.ExtendFootprintTTLResultCodeEnum
                        .EXTEND_FOOTPRINT_TTL_INSUFFICIENT_REFUNDABLE_FEE,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ExtendFootprintTtlInsufficientRefundableFee), false);
    }

    /// <summary>
    ///     Verifies that FromXdr throws ArgumentOutOfRangeException for unknown result code.
    /// </summary>
    [TestMethod]
    public void FromXdr_WithUnknownResultCode_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var xdrResult = new XDR.ExtendFootprintTTLResult
        {
            Discriminant =
            {
                InnerValue = (XDR.ExtendFootprintTTLResultCode.ExtendFootprintTTLResultCodeEnum)999,
            },
        };

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => ExtendFootprintTtlResult.FromXdr(xdrResult));
    }
}

