using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
///     Unit tests for manage offer result types.
/// </summary>
[TestClass]
public class ManageOfferResultTest
{
    /// <summary>
    ///     Verifies that ManageBuyOfferCreated result can be deserialized from XDR correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithBuyOfferCreatedXdr_ReturnsManageBuyOfferCreated()
    {
        // Arrange
        var xdrBase64 =
            "AAAAAAAAAGQAAAAAAAAAAQAAAAAAAAAMAAAAAAAAAAAAAAAAAAAAALtJgdGXASRLp/M5ZpckEa10nJPtYvrgX6M5wTPacDUYAAAAAAAAnIMAAAABWFhYAAAAAAC7SYHRlwEkS6fzOWaXJBGtdJyT7WL64F+jOcEz2nA1GAAAAAAAAAACWgHFAAAAAAoAAABlAAAAAAAAAAAAAAAA";

        // Act
        var transactionResult = (TransactionResultSuccess)TransactionResult.FromXdrBase64(xdrBase64);

        // Assert
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

    /// <summary>
    ///     Verifies that ManageSellOfferCreated result can be deserialized from XDR correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithSellOfferCreatedXdr_ReturnsManageSellOfferCreated()
    {
        // Arrange
        var xdrBase64 =
            "AAAAAACYloD/////AAAAAQAAAAAAAAADAAAAAAAAAAEAAAAAKoNGsl81xj8D8XyekzKZXRuSU2KImhHkQj4QWhroY64AAAAAAAAE0gAAAAAAAAAAAJiWgAAAAAFVU0QAAAAAACqDRrJfNcY/A/F8npMymV0bklNiiJoR5EI+EFoa6GOuAAAAAAADDUAAAAAAAAAAACqDRrJfNcY/A/F8npMymV0bklNiiJoR5EI+EFoa6GOuAAAAAAAABNIAAAAAAAAAAVVTRAAAAAAAKoNGsl81xj8D8XyekzKZXRuSU2KImhHkQj4QWhroY64AAAAAAJiWgAAAA+gAABEYAAAAAQAAAAAAAAAA";

        // Act
        var tx = Utils.AssertResultOfType(xdrBase64, typeof(ManageSellOfferCreated), true);

        // Assert
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

    /// <summary>
    ///     Verifies that ManageSellOfferUpdated result can be deserialized from XDR correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithSellOfferUpdatedXdr_ReturnsManageSellOfferUpdated()
    {
        // Arrange
        var xdrBase64 =
            "AAAAAACYloD/////AAAAAQAAAAAAAAADAAAAAAAAAAEAAAAAKoNGsl81xj8D8XyekzKZXRuSU2KImhHkQj4QWhroY64AAAAAAAAE0gAAAAAAAAAAAJiWgAAAAAFVU0QAAAAAACqDRrJfNcY/A/F8npMymV0bklNiiJoR5EI+EFoa6GOuAAAAAAADDUAAAAABAAAAACqDRrJfNcY/A/F8npMymV0bklNiiJoR5EI+EFoa6GOuAAAAAAAABNIAAAAAAAAAAVVTRAAAAAAAKoNGsl81xj8D8XyekzKZXRuSU2KImhHkQj4QWhroY64AAAAAAJiWgAAAA+gAABEYAAAAAQAAAAAAAAAA";

        // Act
        var tx = Utils.AssertResultOfType(xdrBase64, typeof(ManageSellOfferUpdated), true);

        // Assert
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

    /// <summary>
    ///     Verifies that ManageSellOfferDeleted result with ClaimAtomV0 can be deserialized from XDR correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithSellOfferDeletedClaimAtomV0Xdr_ReturnsManageSellOfferDeletedWithClaimAtomV0()
    {
        // Arrange
        var xdrBase64 =
            "AAAAAACYloD/////AAAAAQAAAAAAAAADAAAAAAAAAAEAAAAAKoNGsl81xj8D8XyekzKZXRuSU2KImhHkQj4QWhroY64AAAAAAAAE0gAAAAAAAAAAAJiWgAAAAAFVU0QAAAAAACqDRrJfNcY/A/F8npMymV0bklNiiJoR5EI+EFoa6GOuAAAAAAADDUAAAAACAAAAAA==";

        // Act
        var tx = Utils.AssertResultOfType(xdrBase64, typeof(ManageSellOfferDeleted), true);

        // Assert
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

    /// <summary>
    ///     Verifies that ManageSellOfferDeleted result with ClaimAtomOrderBook can be deserialized from XDR correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithSellOfferDeletedClaimOrderBookXdr_ReturnsManageSellOfferDeletedWithClaimOrderBook()
    {
        // Arrange
        var xdrBase64 =
            "AAAAAAAAAGQAAAAAAAAAAQAAAAAAAAADAAAAAAAAAAEAAAABAAAAALtJgdGXASRLp/M5ZpckEa10nJPtYvrgX6M5wTPacDUYAAAAAAAAnDoAAAABWFhYAAAAAAC7SYHRlwEkS6fzOWaXJBGtdJyT7WL64F+jOcEz2nA1GAAAAAADk4cAAAAAAAAAAAAC+vCAAAAAAgAAAAA=";

        // Act
        var transactionResult = (TransactionResultSuccess)TransactionResult.FromXdrBase64(xdrBase64);

        // Assert
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

    /// <summary>
    ///     Verifies that ManageBuyOfferDeleted result with ClaimAtomOrderBook can be deserialized from XDR correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithBuyOfferDeletedClaimOrderBookXdr_ReturnsManageBuyOfferDeletedWithClaimOrderBook()
    {
        // Arrange
        var xdrBase64 =
            "AAAAAAAAAGQAAAAAAAAAAQAAAAAAAAAMAAAAAAAAAAEAAAABAAAAALtJgdGXASRLp/M5ZpckEa10nJPtYvrgX6M5wTPacDUYAAAAAAAAnIAAAAABWFhYAAAAAAC7SYHRlwEkS6fzOWaXJBGtdJyT7WL64F+jOcEz2nA1GAAAAAAELB2AAAAAAVlZWQAAAAAA6FIuuSZ2K/XrwkBn5+JDNnnUA9JidV5mVxvQ6AMQF28AAAAABJbtQAAAAAIAAAAA";

        // Act
        var transactionResult = (TransactionResultSuccess)TransactionResult.FromXdrBase64(xdrBase64);

        // Assert
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

    /// <summary>
    ///     Verifies that ManageSellOfferMalformed result can be deserialized from XDR correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageSellOfferMalformedXdr_ReturnsManageSellOfferMalformed()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAD/////wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageSellOfferMalformed), false);
    }

    /// <summary>
    ///     Verifies that ManageSellOfferUnderfunded result can be deserialized from XDR correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithUnderfundedXdr_ReturnsManageSellOfferUnderfunded()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAD////+QAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageSellOfferUnderfunded), false);
    }

    /// <summary>
    ///     Verifies that ManageSellOfferSellNoTrust result can be deserialized from XDR correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageSellOfferSellNoTrustXdr_ReturnsManageSellOfferSellNoTrust()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAD/////gAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageSellOfferSellNoTrust), false);
    }

    /// <summary>
    ///     Verifies that ManageSellOfferBuyNoTrust result can be deserialized from XDR correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithBuyNoTrustXdr_ReturnsManageSellOfferBuyNoTrust()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAD/////QAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageSellOfferBuyNoTrust), false);
    }

    /// <summary>
    ///     Verifies that ManageSellOfferSellNotAuthorized result can be deserialized from XDR correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageSellOfferSellNotAuthorizedXdr_ReturnsManageSellOfferSellNotAuthorized()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAD/////AAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageSellOfferSellNotAuthorized), false);
    }

    /// <summary>
    ///     Verifies that ManageSellOfferBuyNotAuthorized result can be deserialized from XDR correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithBuyNotAuthorizedXdr_ReturnsManageSellOfferBuyNotAuthorized()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAD////+wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageSellOfferBuyNotAuthorized), false);
    }

    /// <summary>
    ///     Verifies that ManageSellOfferLineFull result can be deserialized from XDR correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithLineFullXdr_ReturnsManageSellOfferLineFull()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAD////+gAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageSellOfferLineFull), false);
    }

    /// <summary>
    ///     Verifies that ManageSellOfferCrossSelf result can be deserialized from XDR correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageSellOfferCrossSelfXdr_ReturnsManageSellOfferCrossSelf()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAD////+AAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageSellOfferCrossSelf), false);
    }

    /// <summary>
    ///     Verifies that ManageSellOfferSellNoIssuer result can be deserialized from XDR correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithSellNoIssuerXdr_ReturnsManageSellOfferSellNoIssuer()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAD////9wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageSellOfferSellNoIssuer), false);
    }

    /// <summary>
    ///     Verifies that ManageSellOfferBuyNoIssuer result can be deserialized from XDR correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithBuyNoIssuerXdr_ReturnsManageSellOfferBuyNoIssuer()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAD////9gAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageSellOfferBuyNoIssuer), false);
    }

    /// <summary>
    ///     Verifies that ManageSellOfferNotFound result can be deserialized from XDR correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithNotFoundXdr_ReturnsManageSellOfferNotFound()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAD////9QAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageSellOfferNotFound), false);
    }

    /// <summary>
    ///     Verifies that ManageSellOfferLowReserve result can be deserialized from XDR correctly.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithLowReserveXdr_ReturnsManageSellOfferLowReserve()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAD////9AAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(ManageSellOfferLowReserve), false);
    }

    /// <summary>
    ///     Verifies that OfferEntryFlags are correctly set when deserializing ManageSellOfferCreated result.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_WithOfferEntryFlagsXdr_ReturnsOfferWithCorrectFlags()
    {
        // Arrange
        var xdrBase64 =
            "AAAAAAAAAGQAAAAAAAAAAQAAAAAAAAADAAAAAAAAAAAAAAAAAAAAAM/Ee4SnT3/gopz3ng3SEYJcq/D+9k6K6UsSPJLpqpV3AAAAAAGV4XUAAAABTEtLMQAAAACqysdXjcCwA0NHMgy+BYFMm3s5N8yUziZS4Dge3zQ05QAAAAAAAAAAAcnDgAAAAAEAAAABAAAAAAAAAAAAAAAA";

        // Act
        var result = TransactionResult.FromXdrBase64(xdrBase64);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        var success = (TransactionResultSuccess)result;
        var manageOfferResult = success.Results.First() as ManageSellOfferCreated;
        Assert.IsNotNull(manageOfferResult);
        Assert.IsTrue(manageOfferResult.IsSuccess);
        var offer = manageOfferResult.Offer;
        Assert.IsFalse(offer.Flags.HasFlag(OfferEntry.OfferEntryFlags.PASSIVE));
    }
}