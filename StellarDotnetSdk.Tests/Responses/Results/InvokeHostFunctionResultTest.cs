using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
///     Unit tests for invoke host function result types.
/// </summary>
[TestClass]
public class InvokeHostFunctionResultTest
{
    /// <summary>
    ///     Verifies that InvokeHostFunctionSuccess result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithInvokeHostFunctionSuccessXdr_ReturnsInvokeHostFunctionSuccess()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.INVOKE_HOST_FUNCTION,
            },
            InvokeHostFunctionResult = new XDR.InvokeHostFunctionResult
            {
                Discriminant =
                {
                    InnerValue = XDR.InvokeHostFunctionResultCode.InvokeHostFunctionResultCodeEnum
                        .INVOKE_HOST_FUNCTION_SUCCESS,
                },
                Success = new XDR.Hash(new byte[32]),
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
        Assert.IsInstanceOfType(op, typeof(InvokeHostFunctionSuccess));
        Assert.IsTrue(op.IsSuccess);
    }

    /// <summary>
    ///     Verifies that InvokeHostFunctionMalformed result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithInvokeHostFunctionMalformedXdr_ReturnsInvokeHostFunctionMalformed()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.INVOKE_HOST_FUNCTION,
            },
            InvokeHostFunctionResult = new XDR.InvokeHostFunctionResult
            {
                Discriminant =
                {
                    InnerValue = XDR.InvokeHostFunctionResultCode.InvokeHostFunctionResultCodeEnum
                        .INVOKE_HOST_FUNCTION_MALFORMED,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(InvokeHostFunctionMalformed), false);
    }

    /// <summary>
    ///     Verifies that InvokeHostFunctionTrapped result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithInvokeHostFunctionTrappedXdr_ReturnsInvokeHostFunctionTrapped()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.INVOKE_HOST_FUNCTION,
            },
            InvokeHostFunctionResult = new XDR.InvokeHostFunctionResult
            {
                Discriminant =
                {
                    InnerValue = XDR.InvokeHostFunctionResultCode.InvokeHostFunctionResultCodeEnum
                        .INVOKE_HOST_FUNCTION_TRAPPED,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(InvokeHostFunctionTrapped), false);
    }

    /// <summary>
    ///     Verifies that InvokeHostFunctionResourceLimitExceeded result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithInvokeHostFunctionResourceLimitExceededXdr_ReturnsInvokeHostFunctionResourceLimitExceeded()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.INVOKE_HOST_FUNCTION,
            },
            InvokeHostFunctionResult = new XDR.InvokeHostFunctionResult
            {
                Discriminant =
                {
                    InnerValue = XDR.InvokeHostFunctionResultCode.InvokeHostFunctionResultCodeEnum
                        .INVOKE_HOST_FUNCTION_RESOURCE_LIMIT_EXCEEDED,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(InvokeHostFunctionResourceLimitExceeded), false);
    }

    /// <summary>
    ///     Verifies that InvokeHostFunctionEntryArchived result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithInvokeHostFunctionEntryArchivedXdr_ReturnsInvokeHostFunctionEntryArchived()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.INVOKE_HOST_FUNCTION,
            },
            InvokeHostFunctionResult = new XDR.InvokeHostFunctionResult
            {
                Discriminant =
                {
                    InnerValue = XDR.InvokeHostFunctionResultCode.InvokeHostFunctionResultCodeEnum
                        .INVOKE_HOST_FUNCTION_ENTRY_ARCHIVED,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(InvokeHostFunctionEntryArchived), false);
    }

    /// <summary>
    ///     Verifies that InvokeHostFunctionInsufficientRefundableFee result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithInvokeHostFunctionInsufficientRefundableFeeXdr_ReturnsInvokeHostFunctionInsufficientRefundableFee()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.INVOKE_HOST_FUNCTION,
            },
            InvokeHostFunctionResult = new XDR.InvokeHostFunctionResult
            {
                Discriminant =
                {
                    InnerValue = XDR.InvokeHostFunctionResultCode.InvokeHostFunctionResultCodeEnum
                        .INVOKE_HOST_FUNCTION_INSUFFICIENT_REFUNDABLE_FEE,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(InvokeHostFunctionInsufficientRefundableFee), false);
    }

    /// <summary>
    ///     Verifies that FromXdr throws ArgumentOutOfRangeException for unknown result code.
    /// </summary>
    [TestMethod]
    public void FromXdr_WithUnknownResultCode_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var xdrResult = new XDR.InvokeHostFunctionResult
        {
            Discriminant =
            {
                InnerValue = (XDR.InvokeHostFunctionResultCode.InvokeHostFunctionResultCodeEnum)999,
            },
        };

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => InvokeHostFunctionResult.FromXdr(xdrResult));
    }
}

