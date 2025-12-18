using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
///     Unit tests for path payment strict send result types.
/// </summary>
[TestClass]
public class PathPaymentStrictSendResultTest
{
    /// <summary>
    ///     Verifies that PathPaymentStrictSendSuccess result can be deserialized correctly and contains payment details.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictSendSuccessXdr_ReturnsPathPaymentStrictSendSuccessWithDetails()
    {
        // Arrange
        var xdrBase64 =
            "AAAAAACYloD/////AAAAAQAAAAAAAAANAAAAAAAAAAEAAAAAKoNGsl81xj8D8XyekzKZXRuSU2KImhHkQj4QWhroY64AAAAAAAAE0gAAAAAAAAAAAJiWgAAAAAFVU0QAAAAAACqDRrJfNcY/A/F8npMymV0bklNiiJoR5EI+EFoa6GOuAAAAAAADDUAAAAAAAyzXIcEd0vK9XlVfmjyQE9QpJjOLzYUN5orR0N+Dz+QAAAABVVNEAAAAAAAqg0ayXzXGPwPxfJ6TMpldG5JTYoiaEeRCPhBaGuhjrgAAAAAAAw1AAAAAAA==";

        // Act
        var tx = Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictSendSuccess), true);

        // Assert
        var failed = (TransactionResultFailed)tx;
        var op = (PathPaymentStrictSendSuccess)failed.Results[0];
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
    ///     Verifies that PathPaymentStrictSendMalformed result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictSendMalformedXdr_ReturnsPathPaymentStrictSendMalformed()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAN/////wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictSendMalformed), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictSendUnderfunded result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictSendUnderfundedXdr_ReturnsPathPaymentStrictSendUnderfunded()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAN/////gAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictSendUnderfunded), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictSendSrcNoTrust result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictSendSrcNoTrustXdr_ReturnsPathPaymentStrictSendSrcNoTrust()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAN/////QAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictSendSrcNoTrust), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictSendSrcNotAuthorized result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictSendSrcNotAuthorizedXdr_ReturnsPathPaymentStrictSendSrcNotAuthorized()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAN/////AAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictSendSrcNotAuthorized), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictSendNoDestination result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictSendNoDestinationXdr_ReturnsPathPaymentStrictSendNoDestination()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAN////+wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictSendNoDestination), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictSendNoTrust result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictSendNoTrustXdr_ReturnsPathPaymentStrictSendNoTrust()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAN////+gAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictSendNoTrust), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictSendNotAuthorized result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictSendNotAuthorizedXdr_ReturnsPathPaymentStrictSendNotAuthorized()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAN////+QAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictSendNotAuthorized), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictSendLineFull result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictSendLineFullXdr_ReturnsPathPaymentStrictSendLineFull()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAN////+AAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictSendLineFull), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictSendNoIssuer result can be deserialized correctly and contains no issuer asset.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictSendNoIssuerXdr_ReturnsPathPaymentStrictSendNoIssuerWithAsset()
    {
        // Arrange
        var xdrBase64 =
            "AAAAAACYloD/////AAAAAQAAAAAAAAAN////9wAAAAFVU0QAAAAAACqDRrJfNcY/A/F8npMymV0bklNiiJoR5EI+EFoa6GOuAAAAAA==";

        // Act
        var tx = Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictSendNoIssuer), false);

        // Assert
        var failed = (TransactionResultFailed)tx;
        var op = (PathPaymentStrictSendNoIssuer)failed.Results[0];
        Assert.AreEqual(Asset.CreateNonNativeAsset("USD", "GAVIGRVSL424MPYD6F6J5EZSTFORXESTMKEJUEPEII7BAWQ25BR25DUC"),
            op.NoIssuer);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictSendTooFewOffers result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictSendTooFewOffersXdr_ReturnsPathPaymentStrictSendTooFewOffers()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAN////9gAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictSendTooFewOffers), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictSendOfferCrossSelf result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictSendOfferCrossSelfXdr_ReturnsPathPaymentStrictSendOfferCrossSelf()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAN////9QAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictSendOfferCrossSelf), false);
    }

    /// <summary>
    ///     Verifies that PathPaymentStrictSendUnderDestMin result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathPaymentStrictSendUnderDestMinXdr_ReturnsPathPaymentStrictSendUnderDestMin()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAN////9AAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(PathPaymentStrictSendUnderDestMin), false);
    }
}