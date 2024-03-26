using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using stellar_dotnet_sdk;
using stellar_dotnet_sdk.responses;
using stellar_dotnet_sdk.responses.results;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using Claimant = stellar_dotnet_sdk.Claimant;

namespace stellar_dotnet_sdk_test;

[TestClass]
public class SponsorshipTest
{
    private const string DataName = "my secret";
    private readonly Server _server = new("https://horizon-testnet.stellar.org");

    private readonly KeyPair _sponsoredAccount =
        KeyPair.FromSecretSeed("SDBNUIC2JMIYKGLJUFI743AQDWPBOWKG42GADHEY3FQDTQLJADYPQZTP");

    private readonly KeyPair _sponsoringAccount =
        KeyPair.FromSecretSeed("SDR4PTKMR5TAQQCL3RI2MLXXSXQDIR7DCAONQNQP6UCDZCD4OVRWXUHI");

    private Asset _assetA =
        new AssetTypeCreditAlphaNum4("AAA", "GARRDNS77ZSI6PPXRBWTHIVX4RS2ULVBKNJXFRV77AZUNLDUNV2NAHJA");

    // "GC3TDMFTMYZY2G4C77AKAVC3BR4KL6WMQ6K2MHISKDH2OHRFS7CVVEAF";
    private string SponsoredId => _sponsoredAccount.AccountId;

    // "GARRDNS77ZSI6PPXRBWTHIVX4RS2ULVBKNJXFRV77AZUNLDUNV2NAHJA";
    private string SponsoringId => _sponsoringAccount.AccountId;

    [TestInitialize]
    public async Task Setup()
    {
        Network.UseTestNetwork();

        await TestNetUtil.CheckAndCreateAccountOnTestnet(SponsoringId);
        await TestNetUtil.CheckAndCreateAccountOnTestnet(SponsoredId);

        _assetA = new AssetTypeCreditAlphaNum4("AAA", SponsoringId);
    }

    [TestCleanup]
    public void Cleanup()
    {
        Network.Use(null);
        _server.Dispose();
    }

    private async Task RevokeClaimableBalanceSponsorship(string balanceId)
    {
        Assert.IsNotNull(balanceId);
        var account = await _server.Accounts.Account(SponsoringId);

        var revokeOperation = new RevokeLedgerEntrySponsorshipOperation.Builder(balanceId).Build();

        var beginSponsoringOperation = new BeginSponsoringFutureReservesOperation.Builder(SponsoringId)
            .SetSourceAccount(_sponsoredAccount)
            .Build();
        var endSponsoringOperation = new EndSponsoringFutureReservesOperation.Builder().Build();

        var tx = new TransactionBuilder(account)
            .AddOperation(beginSponsoringOperation)
            .AddOperation(revokeOperation)
            .AddOperation(endSponsoringOperation)
            .Build();
        tx.Sign(_sponsoringAccount);
        tx.Sign(_sponsoredAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess());
        Assert.IsNotNull(txResponse.ResultXdr);
        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr);
        Assert.IsTrue(transactionResult.IsSuccess);
        Assert.IsInstanceOfType(transactionResult, typeof(TransactionResultSuccess));
        Assert.IsNotNull(transactionResult.FeeCharged);
        var results = ((TransactionResultSuccess)transactionResult).Results;
        Assert.AreEqual(3, results.Count);
        Assert.IsInstanceOfType(results[1], typeof(RevokeSponsorshipSuccess));
    }

    private async Task RevokeDataSponsorship()
    {
        var account = await _server.Accounts.Account(SponsoringId);

        var revokeOperation = new RevokeLedgerEntrySponsorshipOperation.Builder(SponsoredId, DataName)
            .Build();

        var tx = new TransactionBuilder(account)
            .AddOperation(revokeOperation)
            .Build();
        tx.Sign(_sponsoringAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess());
        Assert.IsNotNull(txResponse.ResultXdr);
        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr);
        Assert.IsTrue(transactionResult.IsSuccess);
        Assert.IsNotNull(transactionResult.FeeCharged);
        Assert.IsInstanceOfType(transactionResult, typeof(TransactionResultSuccess));
        var results = ((TransactionResultSuccess)transactionResult).Results;
        Assert.AreEqual(1, results.Count);
        Assert.IsInstanceOfType(results[0], typeof(RevokeSponsorshipSuccess));

        account = await _server.Accounts.Account(SponsoredId);

        // Remove the data so the next won't fail next run
        var removeDataOperation =
            new ManageDataOperation.Builder(DataName, (string?)null).SetSourceAccount(_sponsoredAccount).Build();
        tx = new TransactionBuilder(account)
            .AddOperation(removeDataOperation)
            .Build();

        tx.Sign(_sponsoredAccount);
        await _server.SubmitTransaction(tx);
    }

    private async Task RevokeOfferSponsorship(long offerId)
    {
        Assert.IsNotNull(offerId);

        var account = await _server.Accounts.Account(SponsoringId);

        var revokeOperation = new RevokeLedgerEntrySponsorshipOperation.Builder(SponsoredId, offerId)
            .Build();

        var tx = new TransactionBuilder(account)
            .AddOperation(revokeOperation)
            .Build();
        tx.Sign(_sponsoringAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess());
        Assert.IsNotNull(txResponse.ResultXdr);
        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr);
        Assert.IsTrue(transactionResult.IsSuccess);
        Assert.IsNotNull(transactionResult.FeeCharged);
        Assert.IsInstanceOfType(transactionResult, typeof(TransactionResultSuccess));
        var results = ((TransactionResultSuccess)transactionResult).Results;
        Assert.AreEqual(1, results.Count);
        Assert.IsInstanceOfType(results[0], typeof(RevokeSponsorshipSuccess));

        await Task.Delay(2000);
        // Remove the offer so the test won't fail next run
        account = await _server.Accounts.Account(SponsoredId);

        var removeOfferOperation =
            new ManageSellOfferOperation.Builder(new AssetTypeNative(), _assetA, "0", "1.5", offerId)
                .Build();

        // Remove the trustline so the test won't fail the next run
        var removeTrustOperation =
            new ChangeTrustOperation.Builder(_assetA, "0").SetSourceAccount(_sponsoredAccount).Build();
        tx = new TransactionBuilder(account)
            .AddOperation(removeOfferOperation)
            .AddOperation(removeTrustOperation)
            .Build();

        tx.Sign(_sponsoredAccount);

        await _server.SubmitTransaction(tx);
    }

    private async Task RevokeTrustlineSponsorship(Asset asset)
    {
        var account = await _server.Accounts.Account(SponsoringId);

        var revokeOperation = new RevokeLedgerEntrySponsorshipOperation.Builder(SponsoredId, asset)
            .Build();

        var tx = new TransactionBuilder(account)
            .AddOperation(revokeOperation)
            .Build();
        tx.Sign(_sponsoringAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess());
        Assert.IsNotNull(txResponse.ResultXdr);
        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr);
        Assert.IsTrue(transactionResult.IsSuccess);
        Assert.IsNotNull(transactionResult.FeeCharged);
        Assert.IsInstanceOfType(transactionResult, typeof(TransactionResultSuccess));
        var results = ((TransactionResultSuccess)transactionResult).Results;
        Assert.AreEqual(1, results.Count);
        Assert.IsInstanceOfType(results[0], typeof(RevokeSponsorshipSuccess));

        await Task.Delay(2000);
        // Remove the trustline so the test won't fail the next run
        account = await _server.Accounts.Account(SponsoredId);
        // Try removing the trust line if exists
        var removeTrustOperation =
            new ChangeTrustOperation.Builder(asset, "0").SetSourceAccount(_sponsoredAccount).Build();
        tx = new TransactionBuilder(account)
            .AddOperation(removeTrustOperation)
            .Build();

        tx.Sign(_sponsoredAccount);
        await _server.SubmitTransaction(tx);
    }

    private async Task<string> CreateSponsoredClaimableBalance()
    {
        var account = await _server.Accounts.Account(SponsoringId);

        var claimants = new[] { new Claimant(_sponsoringAccount, new ClaimPredicateUnconditional()) };
        var createClaimableBalanceOperation =
            new CreateClaimableBalanceOperation.Builder(new AssetTypeNative(), "10", claimants)
                .SetSourceAccount(_sponsoredAccount).Build();

        var beginSponsoringOperation = new BeginSponsoringFutureReservesOperation.Builder(SponsoredId)
            .Build();
        var endSponsoringOperation = new EndSponsoringFutureReservesOperation.Builder(SponsoredId).Build();

        var tx = new TransactionBuilder(account)
            .AddOperation(beginSponsoringOperation)
            .AddOperation(createClaimableBalanceOperation)
            .AddOperation(endSponsoringOperation)
            .Build();
        tx.Sign(_sponsoringAccount);
        tx.Sign(_sponsoredAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess());
        Assert.IsNotNull(txResponse.ResultXdr);
        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr);
        Assert.IsTrue(transactionResult.IsSuccess);
        Assert.IsNotNull(transactionResult.FeeCharged);
        Assert.IsInstanceOfType(transactionResult, typeof(TransactionResultSuccess));
        var results = ((TransactionResultSuccess)transactionResult).Results;
        Assert.AreEqual(3, results.Count);
        Assert.IsInstanceOfType(results[0], typeof(BeginSponsoringFutureReservesSuccess));
        Assert.IsInstanceOfType(results[1], typeof(CreateClaimableBalanceSuccess));
        Assert.IsInstanceOfType(results[2], typeof(EndSponsoringFutureReservesSuccess));
        var balanceId = ((CreateClaimableBalanceSuccess)results[1]).BalanceId;
        Assert.IsNotNull(balanceId);
        return balanceId;
    }

    private async Task CreateSponsoredData()
    {
        var account = await _server.Accounts.Account(SponsoredId);

        var manageDataOperation = new ManageDataOperation.Builder(DataName, "it's a secret").Build();

        var beginSponsoringOperation = new BeginSponsoringFutureReservesOperation.Builder(SponsoredId)
            .SetSourceAccount(_sponsoringAccount).Build();
        var endSponsoringOperation = new EndSponsoringFutureReservesOperation.Builder(SponsoredId).Build();

        var tx = new TransactionBuilder(account)
            .AddOperation(beginSponsoringOperation)
            .AddOperation(manageDataOperation)
            .AddOperation(endSponsoringOperation)
            .Build();
        tx.Sign(_sponsoringAccount);
        tx.Sign(_sponsoredAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess());
        Assert.IsNotNull(txResponse.ResultXdr);
        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr);
        Assert.IsTrue(transactionResult.IsSuccess);
        Assert.IsNotNull(transactionResult.FeeCharged);
        Assert.IsInstanceOfType(transactionResult, typeof(TransactionResultSuccess));
        var results = ((TransactionResultSuccess)transactionResult).Results;
        Assert.AreEqual(3, results.Count);
        Assert.IsInstanceOfType(results[0], typeof(BeginSponsoringFutureReservesSuccess));
        Assert.IsInstanceOfType(results[1], typeof(ManageDataSuccess));
        Assert.IsInstanceOfType(results[2], typeof(EndSponsoringFutureReservesSuccess));
    }

    private async Task<long> CreateSponsoredOffer()
    {
        var account = await _server.Accounts.Account(SponsoredId);

        var trustOperation = new ChangeTrustOperation.Builder(_assetA).SetSourceAccount(_sponsoredAccount).Build();

        var beginSponsoringOperation = new BeginSponsoringFutureReservesOperation.Builder(SponsoredId)
            .SetSourceAccount(_sponsoringAccount).Build();
        var nativeAsset = new AssetTypeNative();
        const string price = "1.5";
        var manageSellOfferOperation = new ManageSellOfferOperation.Builder(nativeAsset, _assetA, "1", price)
            .SetSourceAccount(_sponsoredAccount).Build();
        var endSponsoringOperation = new EndSponsoringFutureReservesOperation.Builder(SponsoredId).Build();

        var tx = new TransactionBuilder(account)
            .AddOperation(trustOperation)
            .AddOperation(beginSponsoringOperation)
            .AddOperation(manageSellOfferOperation)
            .AddOperation(endSponsoringOperation)
            .Build();
        tx.Sign(_sponsoringAccount);
        tx.Sign(_sponsoredAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess());
        Assert.IsNotNull(txResponse.ResultXdr);
        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr);
        Assert.IsTrue(transactionResult.IsSuccess);
        Assert.IsNotNull(transactionResult.FeeCharged);
        Assert.IsInstanceOfType(transactionResult, typeof(TransactionResultSuccess));
        var results = ((TransactionResultSuccess)transactionResult).Results;
        Assert.AreEqual(4, results.Count);
        Assert.IsInstanceOfType(results[0], typeof(ChangeTrustSuccess));
        Assert.IsInstanceOfType(results[1], typeof(BeginSponsoringFutureReservesSuccess));
        Assert.IsInstanceOfType(results[2], typeof(ManageSellOfferSuccess));
        Assert.IsInstanceOfType(results[3], typeof(EndSponsoringFutureReservesSuccess));
        var createdOffer = (ManageSellOfferCreated)results[2];
        Assert.AreEqual(0, createdOffer.OffersClaimed.Length);
        var offerId = createdOffer.Offer.OfferId;
        Assert.IsNotNull(offerId);
        return offerId;
    }

    private async Task CreateSponsoredTrustline(Asset asset)
    {
        var beginSponsoringOperation = new BeginSponsoringFutureReservesOperation.Builder(SponsoredId)
            .SetSourceAccount(_sponsoringAccount).Build();
        var trustOperation = new ChangeTrustOperation.Builder(asset).SetSourceAccount(_sponsoredAccount).Build();
        var endSponsoringOperation = new EndSponsoringFutureReservesOperation.Builder(SponsoredId).Build();

        var account = await _server.Accounts.Account(SponsoringId);
        var tx = new TransactionBuilder(account)
            .AddOperation(beginSponsoringOperation)
            .AddOperation(trustOperation)
            .AddOperation(endSponsoringOperation)
            .Build();
        tx.Sign(_sponsoringAccount);
        tx.Sign(_sponsoredAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess());
        Assert.IsNotNull(txResponse.ResultXdr);
        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr);
        Assert.IsTrue(transactionResult.IsSuccess);
        Assert.IsNotNull(transactionResult.FeeCharged);
        Assert.IsInstanceOfType(transactionResult, typeof(TransactionResultSuccess));
        var results = ((TransactionResultSuccess)transactionResult).Results;
        Assert.AreEqual(3, results.Count);
        Assert.IsInstanceOfType(results[0], typeof(BeginSponsoringFutureReservesSuccess));
        Assert.IsInstanceOfType(results[1], typeof(ChangeTrustSuccess));
        Assert.IsInstanceOfType(results[2], typeof(EndSponsoringFutureReservesSuccess));
    }

    [TestMethod]
    public async Task TestRevokeSponsorshipAccount()
    {
        var account = await _server.Accounts.Account(SponsoringId);
        var revokeOperation = new RevokeLedgerEntrySponsorshipOperation.Builder(_sponsoringAccount).Build();

        var tx = new TransactionBuilder(account)
            .AddOperation(revokeOperation)
            .Build();
        tx.Sign(_sponsoringAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess());
        Assert.IsNotNull(txResponse.ResultXdr);
        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr);
        Assert.IsTrue(transactionResult.IsSuccess);
        Assert.IsNotNull(transactionResult.FeeCharged);
        Assert.IsInstanceOfType(transactionResult, typeof(TransactionResultSuccess));
        var results = ((TransactionResultSuccess)transactionResult).Results;
        Assert.AreEqual(1, results.Count);
        Assert.IsInstanceOfType(results[0], typeof(RevokeSponsorshipSuccess));
    }

    [TestMethod]
    public async Task TestRevokeSponsorshipClaimableBalance()
    {
        var balanceId = await CreateSponsoredClaimableBalance();
        await Task.Delay(5000);
        await RevokeClaimableBalanceSponsorship(balanceId);
    }

    [TestMethod]
    public async Task TestRevokeSponsorshipData()
    {
        await CreateSponsoredData();
        await Task.Delay(3000);
        await RevokeDataSponsorship();
    }

    [TestMethod]
    public async Task TestRevokeSponsorshipOffer()
    {
        var offerId = await CreateSponsoredOffer();
        await RevokeOfferSponsorship(offerId);
    }

    [TestMethod]
    public async Task TestRevokeSponsorshipTrustline()
    {
        await CreateSponsoredTrustline(_assetA);
        await Task.Delay(5000);
        await RevokeTrustlineSponsorship(_assetA);
    }
}