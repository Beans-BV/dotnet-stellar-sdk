using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
///     Unit tests for liquidity pool deposit result types.
/// </summary>
[TestClass]
public class LiquidityPoolDepositResultTest
{
    /// <summary>
    ///     Verifies that LiquidityPoolDepositSuccess result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithLiquidityPoolDepositSuccessXdr_ReturnsLiquidityPoolDepositSuccess()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.LIQUIDITY_POOL_DEPOSIT,
            },
            LiquidityPoolDepositResult = new XDR.LiquidityPoolDepositResult
            {
                Discriminant =
                {
                    InnerValue = XDR.LiquidityPoolDepositResultCode.LiquidityPoolDepositResultCodeEnum
                        .LIQUIDITY_POOL_DEPOSIT_SUCCESS,
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
        Assert.IsInstanceOfType(op, typeof(LiquidityPoolDepositSuccess));
        Assert.IsTrue(op.IsSuccess);
    }

    /// <summary>
    ///     Verifies that LiquidityPoolDepositMalformed result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithLiquidityPoolDepositMalformedXdr_ReturnsLiquidityPoolDepositMalformed()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.LIQUIDITY_POOL_DEPOSIT,
            },
            LiquidityPoolDepositResult = new XDR.LiquidityPoolDepositResult
            {
                Discriminant =
                {
                    InnerValue = XDR.LiquidityPoolDepositResultCode.LiquidityPoolDepositResultCodeEnum
                        .LIQUIDITY_POOL_DEPOSIT_MALFORMED,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(LiquidityPoolDepositMalformed), false);
    }

    /// <summary>
    ///     Verifies that LiquidityPoolDepositNoTrust result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithLiquidityPoolDepositNoTrustXdr_ReturnsLiquidityPoolDepositNoTrust()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.LIQUIDITY_POOL_DEPOSIT,
            },
            LiquidityPoolDepositResult = new XDR.LiquidityPoolDepositResult
            {
                Discriminant =
                {
                    InnerValue = XDR.LiquidityPoolDepositResultCode.LiquidityPoolDepositResultCodeEnum
                        .LIQUIDITY_POOL_DEPOSIT_NO_TRUST,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(LiquidityPoolDepositNoTrust), false);
    }

    /// <summary>
    ///     Verifies that LiquidityPoolDepositNotAuthorized result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithLiquidityPoolDepositNotAuthorizedXdr_ReturnsLiquidityPoolDepositNotAuthorized()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.LIQUIDITY_POOL_DEPOSIT,
            },
            LiquidityPoolDepositResult = new XDR.LiquidityPoolDepositResult
            {
                Discriminant =
                {
                    InnerValue = XDR.LiquidityPoolDepositResultCode.LiquidityPoolDepositResultCodeEnum
                        .LIQUIDITY_POOL_DEPOSIT_NOT_AUTHORIZED,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(LiquidityPoolDepositNotAuthorized), false);
    }

    /// <summary>
    ///     Verifies that LiquidityPoolDepositUnderfunded result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithLiquidityPoolDepositUnderfundedXdr_ReturnsLiquidityPoolDepositUnderfunded()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.LIQUIDITY_POOL_DEPOSIT,
            },
            LiquidityPoolDepositResult = new XDR.LiquidityPoolDepositResult
            {
                Discriminant =
                {
                    InnerValue = XDR.LiquidityPoolDepositResultCode.LiquidityPoolDepositResultCodeEnum
                        .LIQUIDITY_POOL_DEPOSIT_UNDERFUNDED,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(LiquidityPoolDepositUnderfunded), false);
    }

    /// <summary>
    ///     Verifies that LiquidityPoolDepositLineFull result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithLiquidityPoolDepositLineFullXdr_ReturnsLiquidityPoolDepositLineFull()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.LIQUIDITY_POOL_DEPOSIT,
            },
            LiquidityPoolDepositResult = new XDR.LiquidityPoolDepositResult
            {
                Discriminant =
                {
                    InnerValue = XDR.LiquidityPoolDepositResultCode.LiquidityPoolDepositResultCodeEnum
                        .LIQUIDITY_POOL_DEPOSIT_LINE_FULL,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(LiquidityPoolDepositLineFull), false);
    }

    /// <summary>
    ///     Verifies that LiquidityPoolDepositBadPrice result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithLiquidityPoolDepositBadPriceXdr_ReturnsLiquidityPoolDepositBadPrice()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.LIQUIDITY_POOL_DEPOSIT,
            },
            LiquidityPoolDepositResult = new XDR.LiquidityPoolDepositResult
            {
                Discriminant =
                {
                    InnerValue = XDR.LiquidityPoolDepositResultCode.LiquidityPoolDepositResultCodeEnum
                        .LIQUIDITY_POOL_DEPOSIT_BAD_PRICE,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(LiquidityPoolDepositBadPrice), false);
    }

    /// <summary>
    ///     Verifies that LiquidityPoolDepositPoolFull result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithLiquidityPoolDepositPoolFullXdr_ReturnsLiquidityPoolDepositPoolFull()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.LIQUIDITY_POOL_DEPOSIT,
            },
            LiquidityPoolDepositResult = new XDR.LiquidityPoolDepositResult
            {
                Discriminant =
                {
                    InnerValue = XDR.LiquidityPoolDepositResultCode.LiquidityPoolDepositResultCodeEnum
                        .LIQUIDITY_POOL_DEPOSIT_POOL_FULL,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(LiquidityPoolDepositPoolFull), false);
    }

    /// <summary>
    ///     Verifies that FromXdr throws ArgumentOutOfRangeException for unknown result code.
    /// </summary>
    [TestMethod]
    public void FromXdr_WithUnknownResultCode_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var xdrResult = new XDR.LiquidityPoolDepositResult
        {
            Discriminant =
            {
                InnerValue = (XDR.LiquidityPoolDepositResultCode.LiquidityPoolDepositResultCodeEnum)999,
            },
        };

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => LiquidityPoolDepositResult.FromXdr(xdrResult));
    }
}

