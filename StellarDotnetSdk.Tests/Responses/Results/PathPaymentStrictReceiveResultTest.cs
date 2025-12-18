using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
///     Unit tests for path payment strict receive result types.
/// </summary>
[TestClass]
public class PathPaymentStrictReceiveResultTest
{
    /// <summary>
    ///     Verifies that PathPaymentStrictReceiveSuccess result can be deserialized correctly and contains payment details.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictReceiveSuccessXdr_ReturnsPathPaymentStrictReceiveSuccessWithDetails()
    {
        // Arrange
        var xdrBase64 =
            "AAAAAACYloD/////AAAAAQAAAAAAAAACAAAAAAAAAAEAAAAAKoNGsl81xj8D8XyekzKZXRuSU2KImhHkQj4QWhroY64AAAAAAAAE0gAAAAAAAAAAAJiWgAAAAAFVU0QAAAAAACqDRrJfNcY/A/F8npMymV0bklNiiJoR5EI+EFoa6GOuAAAAAAADDUAAAAAAAyzXIcEd0vK9XlVfmjyQE9QpJjOLzYUN5orR0N+Dz+QAAAABVVNEAAAAAAAqg0ayXzXGPwPxfJ6TMpldG5JTYoiaEeRCPhBaGuhjrgAAAAAAAw1AAAAAAA==";

        // Act
        var tx = Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictReceiveSuccess), true);

        // Assert
        var failed = (TransactionResultFailed)tx;
        var op = (PathPaymentStrictReceiveSuccess)failed.Results[0];
        Assert.AreEqual("GABSZVZBYEO5F4V5LZKV7GR4SAJ5IKJGGOF43BIN42FNDUG7QPH6IMRQ", op.Last.Destination.AccountId);
        Assert.AreEqual(Asset.CreateNonNativeAsset("USD", "GAVIGRVSL424MPYD6F6J5EZSTFORXESTMKEJUEPEII7BAWQ25BR25DUC"),
            op.Last.Asset);
        Assert.AreEqual("0.02", op.Last.Amount);
        Assert.AreEqual(1, op.Offers.Length);
        Assert.IsInstanceOfType(op.Offers[0], typeof(ClaimAtomV0));
        var offer = (ClaimAtomV0)op.Offers[0];
        Assert.AreEqual("GAVIGRVSL424MPYD6F6J5EZSTFORXESTMKEJUEPEII7BAWQ25BR25DUC", offer.Seller.AccountId);
        Assert.AreEqual(1234, offer.OfferId);
        Assert.AreEqual(new AssetTypeNative(), offer.AssetSold);
        Assert.AreEqual("1", offer.AmountSold);
        Assert.AreEqual(Asset.CreateNonNativeAsset("USD", "GAVIGRVSL424MPYD6F6J5EZSTFORXESTMKEJUEPEII7BAWQ25BR25DUC"),
            offer.AssetBought);
        Assert.AreEqual("0.02", offer.AmountBought);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictReceiveMalformed result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictReceiveMalformedXdr_ReturnsPathPaymentStrictReceiveMalformed()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAC/////wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictReceiveMalformed), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictReceiveUnderfunded result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictReceiveUnderfundedXdr_ReturnsPathPaymentStrictReceiveUnderfunded()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAC/////gAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictReceiveUnderfunded), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictReceiveSrcNoTrust result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictReceiveSrcNoTrustXdr_ReturnsPathPaymentStrictReceiveSrcNoTrust()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAC/////QAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictReceiveSrcNoTrust), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictReceiveSrcNotAuthorized result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void
        Deserialize_WithPathPaymentStrictReceiveSrcNotAuthorizedXdr_ReturnsPathPaymentStrictReceiveSrcNotAuthorized()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAC/////AAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictReceiveSrcNotAuthorized), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictReceiveNoDestination result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictReceiveNoDestinationXdr_ReturnsPathPaymentStrictReceiveNoDestination()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAC////+wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictReceiveNoDestination), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictReceiveNoTrust result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictReceiveNoTrustXdr_ReturnsPathPaymentStrictReceiveNoTrust()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAC////+gAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictReceiveNoTrust), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictReceiveNotAuthorized result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictReceiveNotAuthorizedXdr_ReturnsPathPaymentStrictReceiveNotAuthorized()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAC////+QAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictReceiveNotAuthorized), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictReceiveLineFull result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictReceiveLineFullXdr_ReturnsPathPaymentStrictReceiveLineFull()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAC////+AAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictReceiveLineFull), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictReceiveNoIssuer result can be deserialized correctly and contains no issuer asset.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictReceiveNoIssuerXdr_ReturnsPathPaymentStrictReceiveNoIssuerWithAsset()
    {
        // Arrange
        var xdrBase64 =
            "AAAAAACYloD/////AAAAAQAAAAAAAAAC////9wAAAAFVU0QAAAAAACqDRrJfNcY/A/F8npMymV0bklNiiJoR5EI+EFoa6GOuAAAAAA==";

        // Act
        var tx = Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictReceiveNoIssuer), false);

        // Assert
        var failed = (TransactionResultFailed)tx;
        var op = (PathPaymentStrictReceiveNoIssuer)failed.Results[0];
        Assert.AreEqual(Asset.CreateNonNativeAsset("USD", "GAVIGRVSL424MPYD6F6J5EZSTFORXESTMKEJUEPEII7BAWQ25BR25DUC"),
            op.NoIssuer);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictReceiveTooFewOffers result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictReceiveTooFewOffersXdr_ReturnsPathPaymentStrictReceiveTooFewOffers()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAC////9gAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictReceiveTooFewOffers), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictReceiveOfferCrossSelf result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void
        Deserialize_WithPathPaymentStrictReceiveOfferCrossSelfXdr_ReturnsPathPaymentStrictReceiveOfferCrossSelf()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAC////9QAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictReceiveOfferCrossSelf), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictReceiveOverSendmax result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictReceiveOverSendmaxXdr_ReturnsPathPaymentStrictReceiveOverSendmax()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAC////9AAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictReceiveOverSendmax), false);
    }
}