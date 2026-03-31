using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Claimants;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Examples.Horizon;

/// <summary>
///     Examples demonstrating how to revoke sponsorship of various ledger entries
///     on the Stellar test network.
/// </summary>
public static class RevokeSponsorshipExamples
{
    private const string TestNetUrl = "https://horizon-testnet.stellar.org";
    private const string DataName = "my secret";

    public static async Task Run()
    {
        Console.WriteLine("Creating sponsor and sponsored accounts...");
        var sponsorKeyPair = HorizonExamples.CreateKeyPair();
        var sponsoredKeyPair = HorizonExamples.CreateKeyPair();

        await Task.WhenAll(
            HorizonExamples.FundAccountUsingFriendBot(sponsorKeyPair.AccountId),
            HorizonExamples.FundAccountUsingFriendBot(sponsoredKeyPair.AccountId)
        );

        await RevokeAccountSponsorship(sponsorKeyPair);
        await RevokeClaimableBalanceSponsorship(sponsorKeyPair, sponsoredKeyPair);
        await RevokeDataSponsorship(sponsorKeyPair, sponsoredKeyPair);
        await RevokeOfferSponsorship(sponsorKeyPair, sponsoredKeyPair);
        await RevokeTrustlineSponsorship(sponsorKeyPair, sponsoredKeyPair);
    }

    /// <summary>
    ///     Revokes account sponsorship. The sponsor revokes its own account entry's sponsorship.
    /// </summary>
    public static async Task RevokeAccountSponsorship(KeyPair sponsorKeyPair)
    {
        Console.WriteLine("\n--- Revoke Account Sponsorship ---");
        var server = new Server(TestNetUrl);

        var account = await server.Accounts.Account(sponsorKeyPair.AccountId);
        var revokeOperation = RevokeLedgerEntrySponsorshipOperation.ForAccount(sponsorKeyPair);

        var tx = new TransactionBuilder(account)
            .AddOperation(revokeOperation)
            .Build();
        tx.Sign(sponsorKeyPair);

        var txResponse = await HorizonExamples.SubmitTransaction(tx);
        ArgumentNullException.ThrowIfNull(txResponse);

        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr!);
        if (!transactionResult.IsSuccess)
        {
            throw new Exception("Revoke account sponsorship transaction failed.");
        }

        var results = ((TransactionResultSuccess)transactionResult).Results;
        Console.WriteLine($"  Operation results: {results.Count}");
        Console.WriteLine($"  Result type: {results[0].GetType().Name}");
    }

    /// <summary>
    ///     Creates a sponsored claimable balance and then revokes its sponsorship.
    /// </summary>
    public static async Task RevokeClaimableBalanceSponsorship(
        KeyPair sponsorKeyPair,
        KeyPair sponsoredKeyPair)
    {
        Console.WriteLine("\n--- Revoke Claimable Balance Sponsorship ---");

        var balanceId = await CreateSponsoredClaimableBalance(sponsorKeyPair, sponsoredKeyPair);
        Console.WriteLine($"  Created sponsored claimable balance: {balanceId}");
        await Task.Delay(2000);

        var server = new Server(TestNetUrl);
        var account = await server.Accounts.Account(sponsorKeyPair.AccountId);

        var beginSponsoringOperation = new BeginSponsoringFutureReservesOperation(sponsorKeyPair.AccountId,
            sponsoredKeyPair);
        var revokeOperation = RevokeLedgerEntrySponsorshipOperation.ForClaimableBalance(balanceId);
        var endSponsoringOperation = new EndSponsoringFutureReservesOperation();

        var tx = new TransactionBuilder(account)
            .AddOperation(beginSponsoringOperation)
            .AddOperation(revokeOperation)
            .AddOperation(endSponsoringOperation)
            .Build();
        tx.Sign(sponsorKeyPair);
        tx.Sign(sponsoredKeyPair);

        var txResponse = await HorizonExamples.SubmitTransaction(tx);
        ArgumentNullException.ThrowIfNull(txResponse);

        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr!);
        if (!transactionResult.IsSuccess)
        {
            throw new Exception("Revoke claimable balance sponsorship transaction failed.");
        }

        var results = ((TransactionResultSuccess)transactionResult).Results;
        Console.WriteLine($"  Operation results: {results.Count}");
        Console.WriteLine($"  Begin sponsoring: {results[0].GetType().Name}");
        Console.WriteLine($"  Revoke sponsorship: {results[1].GetType().Name}");
        Console.WriteLine($"  End sponsoring: {results[2].GetType().Name}");
    }

    /// <summary>
    ///     Creates a sponsored data entry and then revokes its sponsorship.
    /// </summary>
    public static async Task RevokeDataSponsorship(
        KeyPair sponsorKeyPair,
        KeyPair sponsoredKeyPair)
    {
        Console.WriteLine("\n--- Revoke Data Sponsorship ---");

        await CreateSponsoredData(sponsorKeyPair, sponsoredKeyPair);
        Console.WriteLine("  Created sponsored data entry");
        await Task.Delay(2000);

        var server = new Server(TestNetUrl);
        var account = await server.Accounts.Account(sponsorKeyPair.AccountId);

        var revokeOperation =
            RevokeLedgerEntrySponsorshipOperation.ForData(sponsoredKeyPair.AccountId, DataName);

        var tx = new TransactionBuilder(account)
            .AddOperation(revokeOperation)
            .Build();
        tx.Sign(sponsorKeyPair);

        var txResponse = await HorizonExamples.SubmitTransaction(tx);
        ArgumentNullException.ThrowIfNull(txResponse);

        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr!);
        if (!transactionResult.IsSuccess)
        {
            throw new Exception("Revoke data sponsorship transaction failed.");
        }

        var results = ((TransactionResultSuccess)transactionResult).Results;
        Console.WriteLine($"  Operation results: {results.Count}");
        Console.WriteLine($"  Result type: {results[0].GetType().Name}");

        // Cleanup: remove the data entry
        account = await server.Accounts.Account(sponsoredKeyPair.AccountId);
        var removeDataOperation = new ManageDataOperation(DataName, (string?)null, sponsoredKeyPair);
        tx = new TransactionBuilder(account)
            .AddOperation(removeDataOperation)
            .Build();
        tx.Sign(sponsoredKeyPair);
        await server.SubmitTransaction(tx);
        Console.WriteLine("  Cleaned up data entry");
    }

    /// <summary>
    ///     Creates a sponsored offer and then revokes its sponsorship.
    /// </summary>
    public static async Task RevokeOfferSponsorship(
        KeyPair sponsorKeyPair,
        KeyPair sponsoredKeyPair)
    {
        Console.WriteLine("\n--- Revoke Offer Sponsorship ---");

        var asset = new AssetTypeCreditAlphaNum4("TEST", sponsorKeyPair.AccountId);
        var offerId = await CreateSponsoredOffer(sponsorKeyPair, sponsoredKeyPair, asset);
        Console.WriteLine($"  Created sponsored offer with ID: {offerId}");
        await Task.Delay(2000);

        var server = new Server(TestNetUrl);
        var account = await server.Accounts.Account(sponsorKeyPair.AccountId);

        var revokeOperation =
            RevokeLedgerEntrySponsorshipOperation.ForOffer(sponsoredKeyPair.AccountId, offerId);

        var tx = new TransactionBuilder(account)
            .AddOperation(revokeOperation)
            .Build();
        tx.Sign(sponsorKeyPair);

        var txResponse = await HorizonExamples.SubmitTransaction(tx);
        ArgumentNullException.ThrowIfNull(txResponse);

        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr!);
        if (!transactionResult.IsSuccess)
        {
            throw new Exception("Revoke offer sponsorship transaction failed.");
        }

        var results = ((TransactionResultSuccess)transactionResult).Results;
        Console.WriteLine($"  Operation results: {results.Count}");
        Console.WriteLine($"  Result type: {results[0].GetType().Name}");

        // Cleanup: remove the offer and trustline
        await Task.Delay(2000);
        account = await server.Accounts.Account(sponsoredKeyPair.AccountId);
        var removeOfferOperation =
            new ManageSellOfferOperation(new AssetTypeNative(), asset, "0", "1.5", offerId);
        var removeTrustOperation = new ChangeTrustOperation(asset, "0", sponsoredKeyPair);
        tx = new TransactionBuilder(account)
            .AddOperation(removeOfferOperation)
            .AddOperation(removeTrustOperation)
            .Build();
        tx.Sign(sponsoredKeyPair);
        await server.SubmitTransaction(tx);
        Console.WriteLine("  Cleaned up offer and trustline");
    }

    /// <summary>
    ///     Creates a sponsored trustline and then revokes its sponsorship.
    /// </summary>
    public static async Task RevokeTrustlineSponsorship(
        KeyPair sponsorKeyPair,
        KeyPair sponsoredKeyPair)
    {
        Console.WriteLine("\n--- Revoke Trustline Sponsorship ---");

        var asset = new AssetTypeCreditAlphaNum4("XXXX", sponsorKeyPair.AccountId);
        await CreateSponsoredTrustline(sponsorKeyPair, sponsoredKeyPair, asset);
        Console.WriteLine("  Created sponsored trustline");
        await Task.Delay(2000);

        var server = new Server(TestNetUrl);
        var account = await server.Accounts.Account(sponsorKeyPair.AccountId);

        var revokeOperation =
            RevokeLedgerEntrySponsorshipOperation.ForTrustline(sponsoredKeyPair.AccountId, asset);

        var tx = new TransactionBuilder(account)
            .AddOperation(revokeOperation)
            .Build();
        tx.Sign(sponsorKeyPair);

        var txResponse = await HorizonExamples.SubmitTransaction(tx);
        ArgumentNullException.ThrowIfNull(txResponse);

        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr!);
        if (!transactionResult.IsSuccess)
        {
            throw new Exception("Revoke trustline sponsorship transaction failed.");
        }

        var results = ((TransactionResultSuccess)transactionResult).Results;
        Console.WriteLine($"  Operation results: {results.Count}");
        Console.WriteLine($"  Result type: {results[0].GetType().Name}");

        // Cleanup: remove the trustline
        await Task.Delay(2000);
        account = await server.Accounts.Account(sponsoredKeyPair.AccountId);
        var removeTrustOperation = new ChangeTrustOperation(asset, "0", sponsoredKeyPair);
        tx = new TransactionBuilder(account)
            .AddOperation(removeTrustOperation)
            .Build();
        tx.Sign(sponsoredKeyPair);
        await server.SubmitTransaction(tx);
        Console.WriteLine("  Cleaned up trustline");
    }

    private static async Task<string> CreateSponsoredClaimableBalance(
        KeyPair sponsorKeyPair,
        KeyPair sponsoredKeyPair)
    {
        var server = new Server(TestNetUrl);
        var account = await server.Accounts.Account(sponsorKeyPair.AccountId);

        var claimants = new[] { new Claimant(sponsorKeyPair, new ClaimPredicateUnconditional()) };
        var createClaimableBalanceOperation =
            new CreateClaimableBalanceOperation(new AssetTypeNative(), "10", claimants, sponsoredKeyPair);

        var beginSponsoringOperation = new BeginSponsoringFutureReservesOperation(sponsoredKeyPair.AccountId);
        var endSponsoringOperation = new EndSponsoringFutureReservesOperation(sponsoredKeyPair);

        var tx = new TransactionBuilder(account)
            .AddOperation(beginSponsoringOperation)
            .AddOperation(createClaimableBalanceOperation)
            .AddOperation(endSponsoringOperation)
            .Build();
        tx.Sign(sponsorKeyPair);
        tx.Sign(sponsoredKeyPair);

        var txResponse = await HorizonExamples.SubmitTransaction(tx);
        ArgumentNullException.ThrowIfNull(txResponse);

        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr!);
        if (!transactionResult.IsSuccess)
        {
            throw new Exception("Create sponsored claimable balance failed.");
        }

        var results = ((TransactionResultSuccess)transactionResult).Results;
        var balanceId = ((CreateClaimableBalanceSuccess)results[1]).BalanceId;
        ArgumentNullException.ThrowIfNull(balanceId);
        return balanceId;
    }

    private static async Task CreateSponsoredData(
        KeyPair sponsorKeyPair,
        KeyPair sponsoredKeyPair)
    {
        var server = new Server(TestNetUrl);
        var account = await server.Accounts.Account(sponsoredKeyPair.AccountId);

        var manageDataOperation = new ManageDataOperation(DataName, "it's a secret");

        var beginSponsoringOperation =
            new BeginSponsoringFutureReservesOperation(sponsoredKeyPair.AccountId, sponsorKeyPair);
        var endSponsoringOperation = new EndSponsoringFutureReservesOperation(sponsoredKeyPair);

        var tx = new TransactionBuilder(account)
            .AddOperation(beginSponsoringOperation)
            .AddOperation(manageDataOperation)
            .AddOperation(endSponsoringOperation)
            .Build();
        tx.Sign(sponsorKeyPair);
        tx.Sign(sponsoredKeyPair);

        var txResponse = await HorizonExamples.SubmitTransaction(tx);
        ArgumentNullException.ThrowIfNull(txResponse);

        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr!);
        if (!transactionResult.IsSuccess)
        {
            throw new Exception("Create sponsored data failed.");
        }
    }

    private static async Task<long> CreateSponsoredOffer(
        KeyPair sponsorKeyPair,
        KeyPair sponsoredKeyPair,
        Asset asset)
    {
        var server = new Server(TestNetUrl);
        var account = await server.Accounts.Account(sponsoredKeyPair.AccountId);

        var trustOperation = new ChangeTrustOperation(asset, null, sponsoredKeyPair);

        var beginSponsoringOperation =
            new BeginSponsoringFutureReservesOperation(sponsoredKeyPair.AccountId, sponsorKeyPair);
        var nativeAsset = new AssetTypeNative();
        var manageSellOfferOperation =
            new ManageSellOfferOperation(nativeAsset, asset, "1", "1.5", 0, sponsoredKeyPair);
        var endSponsoringOperation = new EndSponsoringFutureReservesOperation(sponsoredKeyPair);

        var tx = new TransactionBuilder(account)
            .AddOperation(trustOperation)
            .AddOperation(beginSponsoringOperation)
            .AddOperation(manageSellOfferOperation)
            .AddOperation(endSponsoringOperation)
            .Build();
        tx.Sign(sponsorKeyPair);
        tx.Sign(sponsoredKeyPair);

        var txResponse = await HorizonExamples.SubmitTransaction(tx);
        ArgumentNullException.ThrowIfNull(txResponse);

        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr!);
        if (!transactionResult.IsSuccess)
        {
            throw new Exception("Create sponsored offer failed.");
        }

        var results = ((TransactionResultSuccess)transactionResult).Results;
        var createdOffer = (ManageSellOfferCreated)results[2];
        var offerId = createdOffer.Offer.OfferId;
        return offerId;
    }

    private static async Task CreateSponsoredTrustline(
        KeyPair sponsorKeyPair,
        KeyPair sponsoredKeyPair,
        Asset asset)
    {
        var server = new Server(TestNetUrl);

        var beginSponsoringOperation =
            new BeginSponsoringFutureReservesOperation(sponsoredKeyPair.AccountId, sponsorKeyPair);
        var trustOperation = new ChangeTrustOperation(asset, null, sponsoredKeyPair);
        var endSponsoringOperation = new EndSponsoringFutureReservesOperation(sponsoredKeyPair);

        var account = await server.Accounts.Account(sponsorKeyPair.AccountId);
        var tx = new TransactionBuilder(account)
            .AddOperation(beginSponsoringOperation)
            .AddOperation(trustOperation)
            .AddOperation(endSponsoringOperation)
            .Build();
        tx.Sign(sponsorKeyPair);
        tx.Sign(sponsoredKeyPair);

        var txResponse = await HorizonExamples.SubmitTransaction(tx);
        ArgumentNullException.ThrowIfNull(txResponse);

        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr!);
        if (!transactionResult.IsSuccess)
        {
            throw new Exception("Create sponsored trustline failed.");
        }
    }
}
