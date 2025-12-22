using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses.Results;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
///     Unit tests for manage buy offer result types.
/// </summary>
[TestClass]
public class ManageBuyOfferResultTest
{
    /// <summary>
    ///     Verifies that ManageBuyOfferMalformed result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageBuyOfferMalformedXdr_ReturnsManageBuyOfferMalformed()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.MANAGE_BUY_OFFER,
            },
            ManageBuyOfferResult = new XDR.ManageBuyOfferResult
            {
                Discriminant =
                {
                    InnerValue = XDR.ManageBuyOfferResultCode.ManageBuyOfferResultCodeEnum
                        .MANAGE_BUY_OFFER_MALFORMED,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageBuyOfferMalformed), false);
    }

    /// <summary>
    ///     Verifies that ManageBuyOfferUnderfunded result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageBuyOfferUnderfundedXdr_ReturnsManageBuyOfferUnderfunded()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.MANAGE_BUY_OFFER,
            },
            ManageBuyOfferResult = new XDR.ManageBuyOfferResult
            {
                Discriminant =
                {
                    InnerValue = XDR.ManageBuyOfferResultCode.ManageBuyOfferResultCodeEnum
                        .MANAGE_BUY_OFFER_UNDERFUNDED,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageBuyOfferUnderfunded), false);
    }

    /// <summary>
    ///     Verifies that ManageBuyOfferSellNoTrust result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageBuyOfferSellNoTrustXdr_ReturnsManageBuyOfferSellNoTrust()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.MANAGE_BUY_OFFER,
            },
            ManageBuyOfferResult = new XDR.ManageBuyOfferResult
            {
                Discriminant =
                {
                    InnerValue = XDR.ManageBuyOfferResultCode.ManageBuyOfferResultCodeEnum
                        .MANAGE_BUY_OFFER_SELL_NO_TRUST,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageBuyOfferSellNoTrust), false);
    }

    /// <summary>
    ///     Verifies that ManageBuyOfferBuyNoTrust result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageBuyOfferBuyNoTrustXdr_ReturnsManageBuyOfferBuyNoTrust()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.MANAGE_BUY_OFFER,
            },
            ManageBuyOfferResult = new XDR.ManageBuyOfferResult
            {
                Discriminant =
                {
                    InnerValue = XDR.ManageBuyOfferResultCode.ManageBuyOfferResultCodeEnum
                        .MANAGE_BUY_OFFER_BUY_NO_TRUST,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageBuyOfferBuyNoTrust), false);
    }

    /// <summary>
    ///     Verifies that ManageBuyOfferSellNotAuthorized result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageBuyOfferSellNotAuthorizedXdr_ReturnsManageBuyOfferSellNotAuthorized()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.MANAGE_BUY_OFFER,
            },
            ManageBuyOfferResult = new XDR.ManageBuyOfferResult
            {
                Discriminant =
                {
                    InnerValue = XDR.ManageBuyOfferResultCode.ManageBuyOfferResultCodeEnum
                        .MANAGE_BUY_OFFER_SELL_NOT_AUTHORIZED,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageBuyOfferSellNotAuthorized), false);
    }

    /// <summary>
    ///     Verifies that ManageBuyOfferBuyNotAuthorized result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageBuyOfferBuyNotAuthorizedXdr_ReturnsManageBuyOfferBuyNotAuthorized()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.MANAGE_BUY_OFFER,
            },
            ManageBuyOfferResult = new XDR.ManageBuyOfferResult
            {
                Discriminant =
                {
                    InnerValue = XDR.ManageBuyOfferResultCode.ManageBuyOfferResultCodeEnum
                        .MANAGE_BUY_OFFER_BUY_NOT_AUTHORIZED,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageBuyOfferBuyNotAuthorized), false);
    }

    /// <summary>
    ///     Verifies that ManageBuyOfferLineFull result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageBuyOfferLineFullXdr_ReturnsManageBuyOfferLineFull()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.MANAGE_BUY_OFFER,
            },
            ManageBuyOfferResult = new XDR.ManageBuyOfferResult
            {
                Discriminant =
                {
                    InnerValue = XDR.ManageBuyOfferResultCode.ManageBuyOfferResultCodeEnum
                        .MANAGE_BUY_OFFER_LINE_FULL,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageBuyOfferLineFull), false);
    }

    /// <summary>
    ///     Verifies that ManageBuyOfferCrossSelf result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageBuyOfferCrossSelfXdr_ReturnsManageBuyOfferCrossSelf()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.MANAGE_BUY_OFFER,
            },
            ManageBuyOfferResult = new XDR.ManageBuyOfferResult
            {
                Discriminant =
                {
                    InnerValue = XDR.ManageBuyOfferResultCode.ManageBuyOfferResultCodeEnum
                        .MANAGE_BUY_OFFER_CROSS_SELF,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageBuyOfferCrossSelf), false);
    }

    /// <summary>
    ///     Verifies that ManageBuyOfferSellNoIssuer result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageBuyOfferSellNoIssuerXdr_ReturnsManageBuyOfferSellNoIssuer()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.MANAGE_BUY_OFFER,
            },
            ManageBuyOfferResult = new XDR.ManageBuyOfferResult
            {
                Discriminant =
                {
                    InnerValue = XDR.ManageBuyOfferResultCode.ManageBuyOfferResultCodeEnum
                        .MANAGE_BUY_OFFER_SELL_NO_ISSUER,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageBuyOfferSellNoIssuer), false);
    }

    /// <summary>
    ///     Verifies that ManageBuyOfferBuyNoIssuer result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageBuyOfferBuyNoIssuerXdr_ReturnsManageBuyOfferBuyNoIssuer()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.MANAGE_BUY_OFFER,
            },
            ManageBuyOfferResult = new XDR.ManageBuyOfferResult
            {
                Discriminant =
                {
                    InnerValue = XDR.ManageBuyOfferResultCode.ManageBuyOfferResultCodeEnum
                        .MANAGE_BUY_OFFER_BUY_NO_ISSUER,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageBuyOfferBuyNoIssuer), false);
    }

    /// <summary>
    ///     Verifies that ManageBuyOfferNotFound result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageBuyOfferNotFoundXdr_ReturnsManageBuyOfferNotFound()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.MANAGE_BUY_OFFER,
            },
            ManageBuyOfferResult = new XDR.ManageBuyOfferResult
            {
                Discriminant =
                {
                    InnerValue = XDR.ManageBuyOfferResultCode.ManageBuyOfferResultCodeEnum
                        .MANAGE_BUY_OFFER_NOT_FOUND,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageBuyOfferNotFound), false);
    }

    /// <summary>
    ///     Verifies that ManageBuyOfferLowReserve result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageBuyOfferLowReserveXdr_ReturnsManageBuyOfferLowReserve()
    {
        // Arrange
        var operationResultTr = new XDR.OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = XDR.OperationType.OperationTypeEnum.MANAGE_BUY_OFFER,
            },
            ManageBuyOfferResult = new XDR.ManageBuyOfferResult
            {
                Discriminant =
                {
                    InnerValue = XDR.ManageBuyOfferResultCode.ManageBuyOfferResultCodeEnum
                        .MANAGE_BUY_OFFER_LOW_RESERVE,
                },
            },
        };
        var xdrBase64 = Utils.CreateTransactionResultXdr(operationResultTr);

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageBuyOfferLowReserve), false);
    }

    /// <summary>
    ///     Verifies that ManageBuyOfferUpdated.FromXdr can deserialize correctly.
    /// </summary>
    [TestMethod]
    public void FromXdr_WithManageBuyOfferUpdated_ReturnsManageBuyOfferUpdated()
    {
        // Arrange
        var sellerKeyPair = KeyPair.FromAccountId("GC5UTAORS4ASIS5H6M4WNFZECGWXJHET5VRPVYC7UM44CM62OA2RQEPS");
        var xdrOfferEntry = new XDR.OfferEntry
        {
            SellerID = new XDR.AccountID(sellerKeyPair.XdrPublicKey),
            OfferID = new XDR.Int64(12345),
            Selling = new AssetTypeNative().ToXdr(),
            Buying = Asset.CreateNonNativeAsset("USD", "GAVIGRVSL424MPYD6F6J5EZSTFORXESTMKEJUEPEII7BAWQ25BR25DUC").ToXdr(),
            Amount = new XDR.Int64(1000),
            Price = new XDR.Price
            {
                N = new XDR.Int32(1),
                D = new XDR.Int32(2),
            },
            Flags = new XDR.Uint32(0),
            Ext = new XDR.OfferEntry.OfferEntryExt
            {
                Discriminant = 0,
            },
        };
        var successResult = new XDR.ManageOfferSuccessResult
        {
            OffersClaimed = Array.Empty<XDR.ClaimAtom>(),
            Offer = new XDR.ManageOfferSuccessResult.ManageOfferSuccessResultOffer
            {
                Discriminant =
                {
                    InnerValue = XDR.ManageOfferEffect.ManageOfferEffectEnum.MANAGE_OFFER_UPDATED,
                },
                Offer = xdrOfferEntry,
            },
        };

        // Act
        var result = ManageBuyOfferSuccess.FromXdr(successResult);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ManageBuyOfferUpdated));
        var updated = (ManageBuyOfferUpdated)result;
        Assert.IsTrue(updated.IsSuccess);
        Assert.AreEqual(12345, updated.Offer.OfferId);
        Assert.AreEqual(0, updated.OffersClaimed.Length);
    }

    /// <summary>
    ///     Verifies that ManageBuyOfferSuccess.FromXdr throws ArgumentOutOfRangeException for unknown ManageOfferEffect.
    /// </summary>
    [TestMethod]
    public void FromXdr_WithUnknownManageOfferEffect_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var successResult = new XDR.ManageOfferSuccessResult
        {
            OffersClaimed = Array.Empty<XDR.ClaimAtom>(),
            Offer = new XDR.ManageOfferSuccessResult.ManageOfferSuccessResultOffer
            {
                Discriminant =
                {
                    InnerValue = (XDR.ManageOfferEffect.ManageOfferEffectEnum)999,
                },
            },
        };
        var xdrResult = new XDR.ManageBuyOfferResult
        {
            Discriminant =
            {
                InnerValue = XDR.ManageBuyOfferResultCode.ManageBuyOfferResultCodeEnum.MANAGE_BUY_OFFER_SUCCESS,
            },
            Success = successResult,
        };

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => ManageBuyOfferResult.FromXdr(xdrResult));
    }

    /// <summary>
    ///     Verifies that ManageBuyOfferResult.FromXdr throws ArgumentOutOfRangeException for unknown result code.
    /// </summary>
    [TestMethod]
    public void FromXdr_WithUnknownResultCode_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var xdrResult = new XDR.ManageBuyOfferResult
        {
            Discriminant =
            {
                InnerValue = (XDR.ManageBuyOfferResultCode.ManageBuyOfferResultCodeEnum)999,
            },
        };

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => ManageBuyOfferResult.FromXdr(xdrResult));
    }
}

