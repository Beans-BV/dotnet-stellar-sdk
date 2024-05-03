using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class ManageOfferResultTest
{
    [TestMethod]
    public void TestBuyOfferCreated()
    {
        var transactionResult = (TransactionResultSuccess)TransactionResult.FromXdrBase64(
            "AAAAAAAAAGQAAAAAAAAAAQAAAAAAAAAMAAAAAAAAAAAAAAAAAAAAALtJgdGXASRLp/M5ZpckEa10nJPtYvrgX6M5wTPacDUYAAAAAAAAnIMAAAABWFhYAAAAAAC7SYHRlwEkS6fzOWaXJBGtdJyT7WL64F+jOcEz2nA1GAAAAAAAAAACWgHFAAAAAAoAAABlAAAAAAAAAAAAAAAA");
        Assert.IsTrue(transactionResult.IsSuccess);
        Assert.AreEqual(1, transactionResult.Results.Count);
        var op = (ManageBuyOfferCreated)transactionResult.Results[0];
        var offer = op.Offer;
        Assert.AreEqual("GC5UTAORS4ASIS5H6M4WNFZECGWXJHET5VRPVYC7UM44CM62OA2RQEPS", offer.Seller.AccountId);
        Assert.AreEqual(40067, offer.OfferId);
        Assert.AreEqual("XXX:GC5UTAORS4ASIS5H6M4WNFZECGWXJHET5VRPVYC7UM44CM62OA2RQEPS", offer.Selling.CanonicalName());
        Assert.AreEqual(
            new AssetTypeNative(),
            offer.Buying);
        Assert.AreEqual("1010", offer.Amount);
        Assert.AreEqual(new Price(10, 101), offer.Price);

        Assert.AreEqual(0, op.OffersClaimed.Length);
    }

    [TestMethod]
    public void TestSellOfferCreated()
    {
        var tx = Utils.AssertResultOfType(
            "AAAAAACYloD/////AAAAAQAAAAAAAAADAAAAAAAAAAEAAAAAKoNGsl81xj8D8XyekzKZXRuSU2KImhHkQj4QWhroY64AAAAAAAAE0gAAAAAAAAAAAJiWgAAAAAFVU0QAAAAAACqDRrJfNcY/A/F8npMymV0bklNiiJoR5EI+EFoa6GOuAAAAAAADDUAAAAAAAAAAACqDRrJfNcY/A/F8npMymV0bklNiiJoR5EI+EFoa6GOuAAAAAAAABNIAAAAAAAAAAVVTRAAAAAAAKoNGsl81xj8D8XyekzKZXRuSU2KImhHkQj4QWhroY64AAAAAAJiWgAAAA+gAABEYAAAAAQAAAAAAAAAA",
            typeof(ManageSellOfferCreated), true);
        var failed = (TransactionResultFailed)tx;
        var op = (ManageSellOfferCreated)failed.Results[0];
        var offer = op.Offer;
        Assert.AreEqual("GAVIGRVSL424MPYD6F6J5EZSTFORXESTMKEJUEPEII7BAWQ25BR25DUC", offer.Seller.AccountId);
        Assert.AreEqual(1234, offer.OfferId);
        Assert.AreEqual(new AssetTypeNative(), offer.Selling);
        Assert.AreEqual(
            Asset.CreateNonNativeAsset("USD", "GAVIGRVSL424MPYD6F6J5EZSTFORXESTMKEJUEPEII7BAWQ25BR25DUC"),
            offer.Buying);
        Assert.AreEqual("1", offer.Amount);
        Assert.AreEqual(new Price(1000, 4376), offer.Price);

        Assert.AreEqual(1, op.OffersClaimed.Length);
    }

    [TestMethod]
    public void TestSellOfferUpdated()
    {
        var tx = Utils.AssertResultOfType(
            "AAAAAACYloD/////AAAAAQAAAAAAAAADAAAAAAAAAAEAAAAAKoNGsl81xj8D8XyekzKZXRuSU2KImhHkQj4QWhroY64AAAAAAAAE0gAAAAAAAAAAAJiWgAAAAAFVU0QAAAAAACqDRrJfNcY/A/F8npMymV0bklNiiJoR5EI+EFoa6GOuAAAAAAADDUAAAAABAAAAACqDRrJfNcY/A/F8npMymV0bklNiiJoR5EI+EFoa6GOuAAAAAAAABNIAAAAAAAAAAVVTRAAAAAAAKoNGsl81xj8D8XyekzKZXRuSU2KImhHkQj4QWhroY64AAAAAAJiWgAAAA+gAABEYAAAAAQAAAAAAAAAA",
            typeof(ManageSellOfferUpdated), true);
        var failed = (TransactionResultFailed)tx;
        var op = (ManageSellOfferUpdated)failed.Results[0];
        var offer = op.Offer;
        Assert.AreEqual("GAVIGRVSL424MPYD6F6J5EZSTFORXESTMKEJUEPEII7BAWQ25BR25DUC", offer.Seller.AccountId);
        Assert.AreEqual(1234, offer.OfferId);
        Assert.AreEqual(new AssetTypeNative(), offer.Selling);
        Assert.AreEqual(
            Asset.CreateNonNativeAsset("USD", "GAVIGRVSL424MPYD6F6J5EZSTFORXESTMKEJUEPEII7BAWQ25BR25DUC"),
            offer.Buying);
        Assert.AreEqual("1", offer.Amount);
        Assert.AreEqual(new Price(1000, 4376), offer.Price);

        Assert.AreEqual(1, op.OffersClaimed.Length);
    }

    [TestMethod]
    public void TestSellOfferDeletedWithClaimAtomV0()
    {
        var tx = Utils.AssertResultOfType(
            "AAAAAACYloD/////AAAAAQAAAAAAAAADAAAAAAAAAAEAAAAAKoNGsl81xj8D8XyekzKZXRuSU2KImhHkQj4QWhroY64AAAAAAAAE0gAAAAAAAAAAAJiWgAAAAAFVU0QAAAAAACqDRrJfNcY/A/F8npMymV0bklNiiJoR5EI+EFoa6GOuAAAAAAADDUAAAAACAAAAAA==",
            typeof(ManageSellOfferDeleted), true);
        var failed = (TransactionResultFailed)tx;
        var op = (ManageSellOfferDeleted)failed.Results[0];
        Assert.AreEqual(1, op.OffersClaimed.Length);
        var claimAtomV0 = (ClaimAtomV0)op.OffersClaimed[0];
        Assert.AreEqual(1234L, claimAtomV0.OfferId);
        Assert.AreEqual("GAVIGRVSL424MPYD6F6J5EZSTFORXESTMKEJUEPEII7BAWQ25BR25DUC", claimAtomV0.Seller.AccountId);
        Assert.AreEqual("native", claimAtomV0.AssetSold.CanonicalName());
        Assert.AreEqual("1", claimAtomV0.AmountSold);
        Assert.AreEqual("USD:GAVIGRVSL424MPYD6F6J5EZSTFORXESTMKEJUEPEII7BAWQ25BR25DUC",
            claimAtomV0.AssetBought.CanonicalName());
        Assert.AreEqual("0.02", claimAtomV0.AmountBought);
    }

    [TestMethod]
    public void TestSellOfferDeletedWithClaimOrderBook()
    {
        var transactionResult = (TransactionResultSuccess)TransactionResult.FromXdrBase64(
            "AAAAAAAAAGQAAAAAAAAAAQAAAAAAAAADAAAAAAAAAAEAAAABAAAAALtJgdGXASRLp/M5ZpckEa10nJPtYvrgX6M5wTPacDUYAAAAAAAAnDoAAAABWFhYAAAAAAC7SYHRlwEkS6fzOWaXJBGtdJyT7WL64F+jOcEz2nA1GAAAAAADk4cAAAAAAAAAAAAC+vCAAAAAAgAAAAA=");
        Assert.IsTrue(transactionResult.IsSuccess);
        Assert.AreEqual(1, transactionResult.Results.Count);
        var op = (ManageSellOfferDeleted)transactionResult.Results[0];
        Assert.AreEqual(1, op.OffersClaimed.Length);
        var claimAtomV0 = (ClaimAtomOrderBook)op.OffersClaimed[0];
        Assert.AreEqual(39994L, claimAtomV0.OfferId);
        Assert.AreEqual("GC5UTAORS4ASIS5H6M4WNFZECGWXJHET5VRPVYC7UM44CM62OA2RQEPS", claimAtomV0.Seller.AccountId);
        Assert.AreEqual("XXX:GC5UTAORS4ASIS5H6M4WNFZECGWXJHET5VRPVYC7UM44CM62OA2RQEPS",
            claimAtomV0.AssetSold.CanonicalName());
        Assert.AreEqual("6", claimAtomV0.AmountSold);
        Assert.AreEqual("native", claimAtomV0.AssetBought.CanonicalName());
        Assert.AreEqual("5", claimAtomV0.AmountBought);
    }

    [TestMethod]
    public void TestBuyOfferDeletedWithClaimOrderBook()
    {
        var transactionResult = (TransactionResultSuccess)TransactionResult.FromXdrBase64(
            "AAAAAAAAAGQAAAAAAAAAAQAAAAAAAAAMAAAAAAAAAAEAAAABAAAAALtJgdGXASRLp/M5ZpckEa10nJPtYvrgX6M5wTPacDUYAAAAAAAAnIAAAAABWFhYAAAAAAC7SYHRlwEkS6fzOWaXJBGtdJyT7WL64F+jOcEz2nA1GAAAAAAELB2AAAAAAVlZWQAAAAAA6FIuuSZ2K/XrwkBn5+JDNnnUA9JidV5mVxvQ6AMQF28AAAAABJbtQAAAAAIAAAAA");
        Assert.IsTrue(transactionResult.IsSuccess);
        Assert.AreEqual(1, transactionResult.Results.Count);
        var op = (ManageBuyOfferDeleted)transactionResult.Results[0];
        Assert.AreEqual(1, op.OffersClaimed.Length);
        var claimAtomV0 = (ClaimAtomOrderBook)op.OffersClaimed[0];
        Assert.AreEqual(40064L, claimAtomV0.OfferId);
        Assert.AreEqual("GC5UTAORS4ASIS5H6M4WNFZECGWXJHET5VRPVYC7UM44CM62OA2RQEPS", claimAtomV0.Seller.AccountId);
        Assert.AreEqual("XXX:GC5UTAORS4ASIS5H6M4WNFZECGWXJHET5VRPVYC7UM44CM62OA2RQEPS",
            claimAtomV0.AssetSold.CanonicalName());
        Assert.AreEqual("7", claimAtomV0.AmountSold);
        Assert.AreEqual("YYY:GDUFELVZEZ3CX5PLYJAGPZ7CIM3HTVAD2JRHKXTGK4N5B2ADCALW7NGW",
            claimAtomV0.AssetBought.CanonicalName());
        Assert.AreEqual("7.7", claimAtomV0.AmountBought);
    }

    [TestMethod]
    public void TestMalformed()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAD/////wAAAAA=", typeof(ManageSellOfferMalformed),
            false);
    }

    [TestMethod]
    public void TestUnderfunded()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAD////+QAAAAA=", typeof(ManageSellOfferUnderfunded),
            false);
    }

    [TestMethod]
    public void TestSellNoTrust()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAD/////gAAAAA=", typeof(ManageSellOfferSellNoTrust),
            false);
    }

    [TestMethod]
    public void TestBuyNoTrust()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAD/////QAAAAA=", typeof(ManageSellOfferBuyNoTrust),
            false);
    }

    [TestMethod]
    public void TestSellNotAuthorized()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAD/////AAAAAA=",
            typeof(ManageSellOfferSellNotAuthorized), false);
    }

    [TestMethod]
    public void TestBuyNotAuthorized()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAD////+wAAAAA=",
            typeof(ManageSellOfferBuyNotAuthorized),
            false);
    }

    [TestMethod]
    public void TestLineFull()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAD////+gAAAAA=", typeof(ManageSellOfferLineFull),
            false);
    }

    [TestMethod]
    public void TestCrossSelf()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAD////+AAAAAA=", typeof(ManageSellOfferCrossSelf),
            false);
    }

    [TestMethod]
    public void TestSellNoIssuer()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAD////9wAAAAA=", typeof(ManageSellOfferSellNoIssuer),
            false);
    }

    [TestMethod]
    public void TestBuyNoIssuer()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAD////9gAAAAA=", typeof(ManageSellOfferBuyNoIssuer),
            false);
    }

    [TestMethod]
    public void TestNotFound()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAD////9QAAAAA=", typeof(ManageSellOfferNotFound),
            false);
    }

    [TestMethod]
    public void TestLowReserve()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAD////9AAAAAA=", typeof(ManageSellOfferLowReserve),
            false);
    }

    // Test https://github.com/elucidsoft/dotnet-stellar-sdk/issues/180
    [TestMethod]
    public void TestOfferEntryFlagsIsSet()
    {
        var result = TransactionResult.FromXdrBase64(
            "AAAAAAAAAGQAAAAAAAAAAQAAAAAAAAADAAAAAAAAAAAAAAAAAAAAAM/Ee4SnT3/gopz3ng3SEYJcq/D+9k6K6UsSPJLpqpV3AAAAAAGV4XUAAAABTEtLMQAAAACqysdXjcCwA0NHMgy+BYFMm3s5N8yUziZS4Dge3zQ05QAAAAAAAAAAAcnDgAAAAAEAAAABAAAAAAAAAAAAAAAA");
        Assert.IsTrue(result.IsSuccess);
        var success = (TransactionResultSuccess)result;
        var manageOfferResult = success.Results.First() as ManageSellOfferCreated;
        Assert.IsNotNull(manageOfferResult);
        Assert.IsTrue(manageOfferResult.IsSuccess);
        var offer = manageOfferResult.Offer;
        Assert.IsFalse(offer.Flags.HasFlag(OfferEntry.OfferEntryFlags.PASSIVE));
    }
}