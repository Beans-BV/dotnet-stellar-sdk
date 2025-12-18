using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
///     Unit tests for set trustline flags result types.
/// </summary>
[TestClass]
public class SetTrustlineFlagsResultTest
{
    /// <summary>
    ///     Verifies that SetTrustlineFlagsCantRevoke result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSetTrustlineFlagsCantRevokeXdr_ReturnsSetTrustlineFlagsCantRevoke()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.SET_TRUST_LINE_FLAGS,
            },
            SetTrustLineFlagsResult = new XDR.SetTrustLineFlagsResult
            {
                Discriminant =
                {
                    InnerValue = XDR.SetTrustLineFlagsResultCode.SetTrustLineFlagsResultCodeEnum
                        .SET_TRUST_LINE_FLAGS_CANT_REVOKE,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(SetTrustlineFlagsCantRevoke), false);
    }

    /// <summary>
    ///     Verifies that SetTrustlineFlagsInvalidState result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSetTrustlineFlagsInvalidStateXdr_ReturnsSetTrustlineFlagsInvalidState()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.SET_TRUST_LINE_FLAGS,
            },
            SetTrustLineFlagsResult = new XDR.SetTrustLineFlagsResult
            {
                Discriminant =
                {
                    InnerValue = XDR.SetTrustLineFlagsResultCode.SetTrustLineFlagsResultCodeEnum
                        .SET_TRUST_LINE_FLAGS_INVALID_STATE,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(SetTrustlineFlagsInvalidState), false);
    }

    /// <summary>
    ///     Verifies that SetTrustlineFlagsMalformed result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSetTrustlineFlagsMalformedXdr_ReturnsSetTrustlineFlagsMalformed()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.SET_TRUST_LINE_FLAGS,
            },
            SetTrustLineFlagsResult = new XDR.SetTrustLineFlagsResult
            {
                Discriminant =
                {
                    InnerValue = XDR.SetTrustLineFlagsResultCode.SetTrustLineFlagsResultCodeEnum
                        .SET_TRUST_LINE_FLAGS_MALFORMED,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(SetTrustlineFlagsMalformed), false);
    }

    /// <summary>
    ///     Verifies that SetTrustlineFlagsNoTrustline result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSetTrustlineFlagsNoTrustlineXdr_ReturnsSetTrustlineFlagsNoTrustline()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.SET_TRUST_LINE_FLAGS,
            },
            SetTrustLineFlagsResult = new XDR.SetTrustLineFlagsResult
            {
                Discriminant =
                {
                    InnerValue = XDR.SetTrustLineFlagsResultCode.SetTrustLineFlagsResultCodeEnum
                        .SET_TRUST_LINE_FLAGS_NO_TRUST_LINE,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(SetTrustlineFlagsNoTrustline), false);
    }

    /// <summary>
    ///     Verifies that SetTrustlineFlagsSuccess result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSetTrustlineFlagsSuccessXdr_ReturnsSetTrustlineFlagsSuccess()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.SET_TRUST_LINE_FLAGS,
            },
            SetTrustLineFlagsResult = new XDR.SetTrustLineFlagsResult
            {
                Discriminant =
                {
                    InnerValue = XDR.SetTrustLineFlagsResultCode.SetTrustLineFlagsResultCodeEnum
                        .SET_TRUST_LINE_FLAGS_SUCCESS,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(SetTrustlineFlagsSuccess), true);
    }
}