using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Transactions;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace StellarDotnetSdk.Tests;

/// <summary>
///     Unit tests for sponsorship-related transaction result parsing.
/// </summary>
[TestClass]
public class SponsorshipTest
{
    private static readonly KeyPair Signer =
        KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");

    private static Transaction CreateDummyTransaction()
    {
        var account = new Account(Signer.AccountId, 0L);
        var tx = new TransactionBuilder(account)
            .AddOperation(new PaymentOperation(Signer, new AssetTypeNative(), "10"))
            .Build();
        tx.Sign(Signer);
        return tx;
    }

    /// <summary>
    ///     Verifies that a transaction result with RevokeSponsorshipSuccess is parsed correctly.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_RevokeAccountSponsorship_ReturnsSuccess()
    {
        // Arrange
        var resultXdr = Utils.CreateTransactionResultXdr(
            [Utils.CreateRevokeSponsorshipResult()]
        );

        using var server = Utils.CreateTestServerWithContent(
            Utils.BuildSubmitTransactionResponseJson(resultXdr)
        );

        // Act
        var txResponse = await server.SubmitTransaction(
            CreateDummyTransaction(),
            new SubmitTransactionOptions { SkipMemoRequiredCheck = true }
        );

        // Assert
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess);
        Assert.IsNotNull(txResponse.ResultXdr);
        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr);
        Assert.IsTrue(transactionResult.IsSuccess);
        Assert.IsNotNull(transactionResult.FeeCharged);
        Assert.IsInstanceOfType(transactionResult, typeof(TransactionResultSuccess));
        var results = ((TransactionResultSuccess)transactionResult).Results;
        Assert.AreEqual(1, results.Count);
        Assert.IsInstanceOfType(results[0], typeof(RevokeSponsorshipSuccess));
    }

    /// <summary>
    ///     Verifies that a transaction result with BeginSponsoring + CreateClaimableBalance + EndSponsoring
    ///     followed by BeginSponsoring + RevokeSponsorship + EndSponsoring is parsed correctly.
    /// </summary>
    [TestMethod]
    public async Task RevokeClaimableBalanceSponsorship_WithSponsoredBalance_Succeeds()
    {
        // Arrange
        var createResultXdr = Utils.CreateTransactionResultXdr([
            Utils.CreateBeginSponsoringResult(),
            Utils.CreateClaimableBalanceResult(),
            Utils.CreateEndSponsoringResult(),
        ]);

        var revokeResultXdr = Utils.CreateTransactionResultXdr([
            Utils.CreateBeginSponsoringResult(),
            Utils.CreateRevokeSponsorshipResult(),
            Utils.CreateEndSponsoringResult(),
        ]);

        using var server = Utils.CreateTestServerWithResponses(
            Utils.BuildHttpResponse(Utils.BuildSubmitTransactionResponseJson(createResultXdr)),
            Utils.BuildHttpResponse(Utils.BuildSubmitTransactionResponseJson(revokeResultXdr))
        );

        // Act - Submit create
        var txResponse = await server.SubmitTransaction(
            CreateDummyTransaction(),
            new SubmitTransactionOptions { SkipMemoRequiredCheck = true }
        );

        // Assert create
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess);
        var createResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr!);
        Assert.IsTrue(createResult.IsSuccess);
        Assert.IsInstanceOfType(createResult, typeof(TransactionResultSuccess));
        var createResults = ((TransactionResultSuccess)createResult).Results;
        Assert.AreEqual(3, createResults.Count);
        Assert.IsInstanceOfType(createResults[0], typeof(BeginSponsoringFutureReservesSuccess));
        Assert.IsInstanceOfType(createResults[1], typeof(CreateClaimableBalanceSuccess));
        Assert.IsInstanceOfType(createResults[2], typeof(EndSponsoringFutureReservesSuccess));
        var balanceId = ((CreateClaimableBalanceSuccess)createResults[1]).BalanceId;
        Assert.IsNotNull(balanceId);

        // Act - Submit revoke
        txResponse = await server.SubmitTransaction(
            CreateDummyTransaction(),
            new SubmitTransactionOptions { SkipMemoRequiredCheck = true }
        );

        // Assert revoke
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess);
        var revokeResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr!);
        Assert.IsTrue(revokeResult.IsSuccess);
        var revokeResults = ((TransactionResultSuccess)revokeResult).Results;
        Assert.AreEqual(3, revokeResults.Count);
        Assert.IsInstanceOfType(revokeResults[1], typeof(RevokeSponsorshipSuccess));
    }

    /// <summary>
    ///     Verifies that a transaction result with BeginSponsoring + ManageData + EndSponsoring
    ///     followed by RevokeSponsorship is parsed correctly.
    /// </summary>
    [TestMethod]
    public async Task RevokeDataSponsorship_WithSponsoredData_Succeeds()
    {
        // Arrange
        var createResultXdr = Utils.CreateTransactionResultXdr([
            Utils.CreateBeginSponsoringResult(),
            Utils.CreateManageDataResult(),
            Utils.CreateEndSponsoringResult(),
        ]);

        var revokeResultXdr = Utils.CreateTransactionResultXdr(
            [Utils.CreateRevokeSponsorshipResult()]);

        using var server = Utils.CreateTestServerWithResponses(
            Utils.BuildHttpResponse(Utils.BuildSubmitTransactionResponseJson(createResultXdr)),
            Utils.BuildHttpResponse(Utils.BuildSubmitTransactionResponseJson(revokeResultXdr))
        );

        // Act - Submit create
        var txResponse = await server.SubmitTransaction(
            CreateDummyTransaction(),
            new SubmitTransactionOptions { SkipMemoRequiredCheck = true }
        );

        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess);
        var createResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr!);
        Assert.IsTrue(createResult.IsSuccess);
        var createResults = ((TransactionResultSuccess)createResult).Results;
        Assert.AreEqual(3, createResults.Count);
        Assert.IsInstanceOfType(createResults[1], typeof(ManageDataSuccess));

        // Act - Submit revoke
        txResponse = await server.SubmitTransaction(
            CreateDummyTransaction(),
            new SubmitTransactionOptions { SkipMemoRequiredCheck = true }
        );

        // Assert
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess);
        var revokeResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr!);
        Assert.IsTrue(revokeResult.IsSuccess);
        Assert.IsNotNull(revokeResult.FeeCharged);
        Assert.IsInstanceOfType(revokeResult, typeof(TransactionResultSuccess));
        var revokeResults = ((TransactionResultSuccess)revokeResult).Results;
        Assert.AreEqual(1, revokeResults.Count);
        Assert.IsInstanceOfType(revokeResults[0], typeof(RevokeSponsorshipSuccess));
    }

    /// <summary>
    ///     Verifies that a transaction result with ChangeTrust + BeginSponsoring + ManageSellOfferCreated + EndSponsoring
    ///     followed by RevokeSponsorship is parsed correctly.
    /// </summary>
    [TestMethod]
    public async Task RevokeOfferSponsorship_WithSponsoredOffer_Succeeds()
    {
        // Arrange
        const long testOfferId = 12345L;

        var createResultXdr = Utils.CreateTransactionResultXdr([
            Utils.CreateChangeTrustResult(),
            Utils.CreateBeginSponsoringResult(),
            Utils.CreateManageSellOfferCreatedResult(testOfferId, Signer.AccountId),
            Utils.CreateEndSponsoringResult(),
        ]);

        var revokeResultXdr = Utils.CreateTransactionResultXdr(
            [Utils.CreateRevokeSponsorshipResult()]
        );

        using var server = Utils.CreateTestServerWithResponses(
            Utils.BuildHttpResponse(Utils.BuildSubmitTransactionResponseJson(createResultXdr)),
            Utils.BuildHttpResponse(Utils.BuildSubmitTransactionResponseJson(revokeResultXdr))
        );

        // Act - Submit create
        var txResponse = await server.SubmitTransaction(
            CreateDummyTransaction(),
            new SubmitTransactionOptions { SkipMemoRequiredCheck = true }
        );

        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess);
        var createResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr!);
        Assert.IsTrue(createResult.IsSuccess);
        var createResults = ((TransactionResultSuccess)createResult).Results;
        Assert.AreEqual(4, createResults.Count);
        Assert.IsInstanceOfType(createResults[0], typeof(ChangeTrustSuccess));
        Assert.IsInstanceOfType(createResults[1], typeof(BeginSponsoringFutureReservesSuccess));
        Assert.IsInstanceOfType(createResults[2], typeof(ManageSellOfferCreated));
        Assert.IsInstanceOfType(createResults[3], typeof(EndSponsoringFutureReservesSuccess));
        var createdOffer = (ManageSellOfferCreated)createResults[2];
        Assert.AreEqual(0, createdOffer.OffersClaimed.Length);
        var offerId = createdOffer.Offer.OfferId;
        Assert.IsNotNull(offerId);

        // Act - Submit revoke
        txResponse = await server.SubmitTransaction(
            CreateDummyTransaction(),
            new SubmitTransactionOptions { SkipMemoRequiredCheck = true }
        );

        // Assert
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess);
        var revokeResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr!);
        Assert.IsTrue(revokeResult.IsSuccess);
        Assert.IsNotNull(revokeResult.FeeCharged);
        Assert.IsInstanceOfType(revokeResult, typeof(TransactionResultSuccess));
        var revokeResults = ((TransactionResultSuccess)revokeResult).Results;
        Assert.AreEqual(1, revokeResults.Count);
        Assert.IsInstanceOfType(revokeResults[0], typeof(RevokeSponsorshipSuccess));
    }

    /// <summary>
    ///     Verifies that a transaction result with BeginSponsoring + ChangeTrust + EndSponsoring
    ///     followed by RevokeSponsorship is parsed correctly.
    /// </summary>
    [TestMethod]
    public async Task RevokeTrustlineSponsorship_WithSponsoredTrustline_Succeeds()
    {
        // Arrange
        var createResultXdr = Utils.CreateTransactionResultXdr([
            Utils.CreateBeginSponsoringResult(),
            Utils.CreateChangeTrustResult(),
            Utils.CreateEndSponsoringResult(),
        ]);

        var revokeResultXdr = Utils.CreateTransactionResultXdr(
            [Utils.CreateRevokeSponsorshipResult()]
        );

        using var server = Utils.CreateTestServerWithResponses(
            Utils.BuildHttpResponse(Utils.BuildSubmitTransactionResponseJson(createResultXdr)),
            Utils.BuildHttpResponse(Utils.BuildSubmitTransactionResponseJson(revokeResultXdr))
        );

        // Act - Submit create
        var txResponse = await server.SubmitTransaction(
            CreateDummyTransaction(),
            new SubmitTransactionOptions { SkipMemoRequiredCheck = true }
        );

        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess);
        var createResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr!);
        Assert.IsTrue(createResult.IsSuccess);
        var createResults = ((TransactionResultSuccess)createResult).Results;
        Assert.AreEqual(3, createResults.Count);
        Assert.IsInstanceOfType(createResults[0], typeof(BeginSponsoringFutureReservesSuccess));
        Assert.IsInstanceOfType(createResults[1], typeof(ChangeTrustSuccess));
        Assert.IsInstanceOfType(createResults[2], typeof(EndSponsoringFutureReservesSuccess));

        // Act - Submit revoke
        txResponse = await server.SubmitTransaction(
            CreateDummyTransaction(),
            new SubmitTransactionOptions { SkipMemoRequiredCheck = true }
        );

        // Assert
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess);
        var revokeResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr!);
        Assert.IsTrue(revokeResult.IsSuccess);
        Assert.IsNotNull(revokeResult.FeeCharged);
        Assert.IsInstanceOfType(revokeResult, typeof(TransactionResultSuccess));
        var revokeResults = ((TransactionResultSuccess)revokeResult).Results;
        Assert.AreEqual(1, revokeResults.Count);
        Assert.IsInstanceOfType(revokeResults[0], typeof(RevokeSponsorshipSuccess));
    }
}