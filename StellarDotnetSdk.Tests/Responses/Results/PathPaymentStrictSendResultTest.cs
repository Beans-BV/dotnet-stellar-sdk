using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class PathPaymentStrictSendResultTest
{
    [TestMethod]
    public void TestSuccess()
    {
        var tx = Utils.AssertResultOfType(
            "AAAAAACYloD/////AAAAAQAAAAAAAAANAAAAAAAAAAEAAAAAKoNGsl81xj8D8XyekzKZXRuSU2KImhHkQj4QWhroY64AAAAAAAAE0gAAAAAAAAAAAJiWgAAAAAFVU0QAAAAAACqDRrJfNcY/A/F8npMymV0bklNiiJoR5EI+EFoa6GOuAAAAAAADDUAAAAAAAyzXIcEd0vK9XlVfmjyQE9QpJjOLzYUN5orR0N+Dz+QAAAABVVNEAAAAAAAqg0ayXzXGPwPxfJ6TMpldG5JTYoiaEeRCPhBaGuhjrgAAAAAAAw1AAAAAAA==",
            typeof(PathPaymentStrictSendSuccess), true);
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

    [TestMethod]
    public void TestMalformed()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAN/////wAAAAA=",
            typeof(PathPaymentStrictSendMalformed),
            false);
    }

    [TestMethod]
    public void TestUnderfunded()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAN/////gAAAAA=",
            typeof(PathPaymentStrictSendUnderfunded),
            false);
    }

    [TestMethod]
    public void TestSrcNoTrust()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAN/////QAAAAA=",
            typeof(PathPaymentStrictSendSrcNoTrust),
            false);
    }

    [TestMethod]
    public void TestSrcNotAuthorized()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAN/////AAAAAA=",
            typeof(PathPaymentStrictSendSrcNotAuthorized),
            false);
    }

    [TestMethod]
    public void TestNoDestination()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAN////+wAAAAA=",
            typeof(PathPaymentStrictSendNoDestination),
            false);
    }

    [TestMethod]
    public void TestNoTrust()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAN////+gAAAAA=",
            typeof(PathPaymentStrictSendNoTrust),
            false);
    }

    [TestMethod]
    public void TestNotAuthorized()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAN////+QAAAAA=",
            typeof(PathPaymentStrictSendNotAuthorized),
            false);
    }

    [TestMethod]
    public void TestLineFull()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAN////+AAAAAA=",
            typeof(PathPaymentStrictSendLineFull),
            false);
    }

    [TestMethod]
    public void TestNoIssuer()
    {
        var tx = Utils.AssertResultOfType(
            "AAAAAACYloD/////AAAAAQAAAAAAAAAN////9wAAAAFVU0QAAAAAACqDRrJfNcY/A/F8npMymV0bklNiiJoR5EI+EFoa6GOuAAAAAA==",
            typeof(PathPaymentStrictSendNoIssuer), false);
        var failed = (TransactionResultFailed)tx;
        var op = (PathPaymentStrictSendNoIssuer)failed.Results[0];
        Assert.AreEqual(Asset.CreateNonNativeAsset("USD", "GAVIGRVSL424MPYD6F6J5EZSTFORXESTMKEJUEPEII7BAWQ25BR25DUC"),
            op.NoIssuer);
    }

    [TestMethod]
    public void TestTooFewOffer()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAN////9gAAAAA=",
            typeof(PathPaymentStrictSendTooFewOffers),
            false);
    }

    [TestMethod]
    public void TestOfferCrossSelf()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAN////9QAAAAA=",
            typeof(PathPaymentStrictSendOfferCrossSelf),
            false);
    }

    [TestMethod]
    public void TestUnderDestMin()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAN////9AAAAAA=",
            typeof(PathPaymentStrictSendUnderDestMin),
            false);
    }
}