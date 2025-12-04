using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Claimants;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Transactions;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace StellarDotnetSdk.Tests;

[TestClass]
public class SponsorshipTest
{
    private const string DataName = "my secret";

    private readonly Asset _assetB =
        new AssetTypeCreditAlphaNum4("XXXX", "GC5UTAORS4ASIS5H6M4WNFZECGWXJHET5VRPVYC7UM44CM62OA2RQEPS");

    private readonly Server _server = new("https://horizon-testnet.stellar.org");

    private readonly KeyPair _sponsoredAccount =
        KeyPair.FromSecretSeed("SBV33ITENGZRQ3UEUY5XD3NOBHHSGZY2ADF2OQ7JC2FR2S3BV3DSHEGC");

    private readonly KeyPair _sponsoringAccount =
        KeyPair.FromSecretSeed("SBQZZETKBHMRVNPEM7TMYAXORIRIDBBS6HD43C3PFH75SI54QAC6YTE2");

    private Asset _assetA =
        new AssetTypeCreditAlphaNum4("XXXY", "GC5UTAORS4ASIS5H6M4WNFZECGWXJHET5VRPVYC7UM44CM62OA2RQEPS");

    // "GDUFELVZEZ3CX5PLYJAGPZ7CIM3HTVAD2JRHKXTGK4N5B2ADCALW7NGW";
    private string SponsoredId => _sponsoredAccount.AccountId;

    // "GC5UTAORS4ASIS5H6M4WNFZECGWXJHET5VRPVYC7UM44CM62OA2RQEPS";
    private string SponsoringId => _sponsoringAccount.AccountId;

    [TestInitialize]
    public async Task Setup()
    {
        Network.UseTestNetwork();

        await Utils.CheckAndCreateAccountOnTestnet(SponsoringId);
        await Utils.CheckAndCreateAccountOnTestnet(SponsoredId);

        _assetA = new AssetTypeCreditAlphaNum4("XXXY", SponsoringId);
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

        var revokeOperation = RevokeLedgerEntrySponsorshipOperation.ForClaimableBalance(balanceId);

        var beginSponsoringOperation = new BeginSponsoringFutureReservesOperation(SponsoringId, _sponsoredAccount);
        var endSponsoringOperation = new EndSponsoringFutureReservesOperation();

        var tx = new TransactionBuilder(account)
            .AddOperation(beginSponsoringOperation)
            .AddOperation(revokeOperation)
            .AddOperation(endSponsoringOperation)
            .Build();
        tx.Sign(_sponsoringAccount);
        tx.Sign(_sponsoredAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess);
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

        var revokeOperation = RevokeLedgerEntrySponsorshipOperation.ForData(SponsoredId, DataName);

        var tx = new TransactionBuilder(account)
            .AddOperation(revokeOperation)
            .Build();
        tx.Sign(_sponsoringAccount);

        var txResponse = await _server.SubmitTransaction(tx);
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

        account = await _server.Accounts.Account(SponsoredId);

        // Remove the data so the next won't fail next run
        var removeDataOperation =
            new ManageDataOperation(DataName, (string?)null, _sponsoredAccount);
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

        var revokeOperation = RevokeLedgerEntrySponsorshipOperation.ForOffer(SponsoredId, offerId);

        var tx = new TransactionBuilder(account)
            .AddOperation(revokeOperation)
            .Build();
        tx.Sign(_sponsoringAccount);

        var txResponse = await _server.SubmitTransaction(tx);
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

        await Task.Delay(2000);
        // Remove the offer so the test won't fail next run
        account = await _server.Accounts.Account(SponsoredId);

        var removeOfferOperation =
            new ManageSellOfferOperation(new AssetTypeNative(), _assetA, "0", "1.5", offerId);

        // Remove the trustline so the test won't fail the next run
        var removeTrustOperation =
            new ChangeTrustOperation(_assetA, "0", _sponsoredAccount);
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

        var revokeOperation = RevokeLedgerEntrySponsorshipOperation.ForTrustline(SponsoredId, asset);

        var tx = new TransactionBuilder(account)
            .AddOperation(revokeOperation)
            .Build();
        tx.Sign(_sponsoringAccount);

        var txResponse = await _server.SubmitTransaction(tx);
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

        await Task.Delay(2000);
        // Remove the trustline so the test won't fail the next run
        account = await _server.Accounts.Account(SponsoredId);
        // Try removing the trust line if exists
        var removeTrustOperation =
            new ChangeTrustOperation(asset, "0", _sponsoredAccount);
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
            new CreateClaimableBalanceOperation(new AssetTypeNative(), "10", claimants, _sponsoredAccount);

        var beginSponsoringOperation = new BeginSponsoringFutureReservesOperation(SponsoredId);
        var endSponsoringOperation = new EndSponsoringFutureReservesOperation(_sponsoredAccount);

        var tx = new TransactionBuilder(account)
            .AddOperation(beginSponsoringOperation)
            .AddOperation(createClaimableBalanceOperation)
            .AddOperation(endSponsoringOperation)
            .Build();
        tx.Sign(_sponsoringAccount);
        tx.Sign(_sponsoredAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess);
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

        var manageDataOperation = new ManageDataOperation(DataName, "it's a secret");

        var beginSponsoringOperation = new BeginSponsoringFutureReservesOperation(SponsoredId, _sponsoringAccount);
        var endSponsoringOperation = new EndSponsoringFutureReservesOperation(_sponsoredAccount);

        var tx = new TransactionBuilder(account)
            .AddOperation(beginSponsoringOperation)
            .AddOperation(manageDataOperation)
            .AddOperation(endSponsoringOperation)
            .Build();
        tx.Sign(_sponsoringAccount);
        tx.Sign(_sponsoredAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess);
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

        var trustOperation = new ChangeTrustOperation(_assetA, null, _sponsoredAccount);

        var beginSponsoringOperation = new BeginSponsoringFutureReservesOperation(SponsoredId, _sponsoringAccount);
        var nativeAsset = new AssetTypeNative();
        const string price = "1.5";
        var manageSellOfferOperation =
            new ManageSellOfferOperation(nativeAsset, _assetA, "1", price, 0, _sponsoredAccount);
        var endSponsoringOperation = new EndSponsoringFutureReservesOperation(_sponsoredAccount);

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
        Assert.IsTrue(txResponse.IsSuccess);
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
        var beginSponsoringOperation = new BeginSponsoringFutureReservesOperation(SponsoredId, _sponsoringAccount);
        var trustOperation = new ChangeTrustOperation(asset, null, _sponsoredAccount);
        var endSponsoringOperation = new EndSponsoringFutureReservesOperation(_sponsoredAccount);

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
        Assert.IsTrue(txResponse.IsSuccess);
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
        var revokeOperation = RevokeLedgerEntrySponsorshipOperation.ForAccount(_sponsoringAccount);

        var tx = new TransactionBuilder(account)
            .AddOperation(revokeOperation)
            .Build();
        tx.Sign(_sponsoringAccount);

        var txResponse = await _server.SubmitTransaction(tx);
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

    [TestMethod]
    public async Task TestRevokeSponsorshipClaimableBalance()
    {
        var balanceId = await CreateSponsoredClaimableBalance();
        await Task.Delay(2000);
        await RevokeClaimableBalanceSponsorship(balanceId);
    }

    [TestMethod]
    public async Task TestRevokeSponsorshipData()
    {
        await CreateSponsoredData();
        await Task.Delay(2000);
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
        await CreateSponsoredTrustline(_assetB);
        await Task.Delay(2000);
        await RevokeTrustlineSponsorship(_assetB);
    }
}