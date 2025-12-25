using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
///     Unit tests for liquidity pool withdraw result types.
/// </summary>
[TestClass]
public class LiquidityPoolWithdrawResultTest
{
    /// <summary>
    ///     Verifies that LiquidityPoolWithdrawSuccess result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithLiquidityPoolWithdrawSuccessXdr_ReturnsLiquidityPoolWithdrawSuccess()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.LIQUIDITY_POOL_WITHDRAW,
            },
            LiquidityPoolWithdrawResult = new XDR.LiquidityPoolWithdrawResult
            {
                Discriminant =
                {
                    InnerValue = XDR.LiquidityPoolWithdrawResultCode.LiquidityPoolWithdrawResultCodeEnum
                        .LIQUIDITY_POOL_WITHDRAW_SUCCESS,
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
        Assert.IsInstanceOfType(op, typeof(LiquidityPoolWithdrawSuccess));
        Assert.IsTrue(op.IsSuccess);
    }

    /// <summary>
    ///     Verifies that LiquidityPoolWithdrawMalformed result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithLiquidityPoolWithdrawMalformedXdr_ReturnsLiquidityPoolWithdrawMalformed()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.LIQUIDITY_POOL_WITHDRAW,
            },
            LiquidityPoolWithdrawResult = new XDR.LiquidityPoolWithdrawResult
            {
                Discriminant =
                {
                    InnerValue = XDR.LiquidityPoolWithdrawResultCode.LiquidityPoolWithdrawResultCodeEnum
                        .LIQUIDITY_POOL_WITHDRAW_MALFORMED,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(LiquidityPoolWithdrawMalformed), false);
    }

    /// <summary>
    ///     Verifies that LiquidityPoolWithdrawNoTrust result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithLiquidityPoolWithdrawNoTrustXdr_ReturnsLiquidityPoolWithdrawNoTrust()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.LIQUIDITY_POOL_WITHDRAW,
            },
            LiquidityPoolWithdrawResult = new XDR.LiquidityPoolWithdrawResult
            {
                Discriminant =
                {
                    InnerValue = XDR.LiquidityPoolWithdrawResultCode.LiquidityPoolWithdrawResultCodeEnum
                        .LIQUIDITY_POOL_WITHDRAW_NO_TRUST,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(LiquidityPoolWithdrawNoTrust), false);
    }

    /// <summary>
    ///     Verifies that LiquidityPoolWithdrawUnderfunded result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithLiquidityPoolWithdrawUnderfundedXdr_ReturnsLiquidityPoolWithdrawUnderfunded()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.LIQUIDITY_POOL_WITHDRAW,
            },
            LiquidityPoolWithdrawResult = new XDR.LiquidityPoolWithdrawResult
            {
                Discriminant =
                {
                    InnerValue = XDR.LiquidityPoolWithdrawResultCode.LiquidityPoolWithdrawResultCodeEnum
                        .LIQUIDITY_POOL_WITHDRAW_UNDERFUNDED,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(LiquidityPoolWithdrawUnderfunded), false);
    }

    /// <summary>
    ///     Verifies that LiquidityPoolWithdrawLineFull result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithLiquidityPoolWithdrawLineFullXdr_ReturnsLiquidityPoolWithdrawLineFull()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.LIQUIDITY_POOL_WITHDRAW,
            },
            LiquidityPoolWithdrawResult = new XDR.LiquidityPoolWithdrawResult
            {
                Discriminant =
                {
                    InnerValue = XDR.LiquidityPoolWithdrawResultCode.LiquidityPoolWithdrawResultCodeEnum
                        .LIQUIDITY_POOL_WITHDRAW_LINE_FULL,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(LiquidityPoolWithdrawLineFull), false);
    }

    /// <summary>
    ///     Verifies that LiquidityPoolWithdrawUnderMinimum result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithLiquidityPoolWithdrawUnderMinimumXdr_ReturnsLiquidityPoolWithdrawUnderMinimum()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.LIQUIDITY_POOL_WITHDRAW,
            },
            LiquidityPoolWithdrawResult = new XDR.LiquidityPoolWithdrawResult
            {
                Discriminant =
                {
                    InnerValue = XDR.LiquidityPoolWithdrawResultCode.LiquidityPoolWithdrawResultCodeEnum
                        .LIQUIDITY_POOL_WITHDRAW_UNDER_MINIMUM,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(LiquidityPoolWithdrawUnderMinimum), false);
    }

    /// <summary>
    ///     Verifies that FromXdr throws ArgumentOutOfRangeException for unknown result code.
    /// </summary>
    [TestMethod]
    public void FromXdr_WithUnknownResultCode_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var xdrResult = new XDR.LiquidityPoolWithdrawResult
        {
            Discriminant =
            {
                InnerValue = (XDR.LiquidityPoolWithdrawResultCode.LiquidityPoolWithdrawResultCodeEnum)999,
            },
        };

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => LiquidityPoolWithdrawResult.FromXdr(xdrResult));
    }
}

