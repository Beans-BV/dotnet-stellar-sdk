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
///     Sponsorship revocation behaves differently depending on the ledger entry type:
///     - Account, data, offer, and trustline entries: the current sponsor can simply revoke
///     the sponsorship in a single operation - the sponsored account then pays its own reserves.
///     - Claimable balance entries: sponsorship can only be TRANSFERRED to another account,
///     never removed outright. This requires wrapping the revoke operation in a
///     BeginSponsoringFutureReserves/EndSponsoringFutureReserves block.
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
    ///     Account entries allow simple revocation - no transfer needed.
    /// </summary>
    public static async Task RevokeAccountSponsorship(KeyPair sponsorKeyPair)
    {
        Console.WriteLine("\n--- Revoke Account Sponsorship ---");
        var server = new Server(TestNetUrl);

        var account = await server.Accounts.Account(sponsorKeyPair.AccountId);

        // A single revoke operation is enough for account entries.
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
    ///     Creates a sponsored claimable balance and then transfers its sponsorship.
    ///     Unlike other ledger entry types, claimable balance sponsorship cannot be simply
    ///     revoked - it can only be transferred to another account (Stellar will return
    ///     "REVOKE_SPONSORSHIP_ONLY_TRANSFERABLE" error if the user tries a plain revoke).
    ///     To transfer, wrap the revoke operation in a
    ///     BeginSponsoringFutureReserves/EndSponsoringFutureReserves block where the
    ///     new sponsor agrees to take over.
    /// </summary>
    public static async Task RevokeClaimableBalanceSponsorship(
        KeyPair sponsorKeyPair,
        KeyPair sponsoredKeyPair)
    {
        Console.WriteLine("\n--- Revoke Claimable Balance Sponsorship ---");

        // Step 1: Create a claimable balance sponsored by sponsorKeyPair.
        var balanceId = await CreateSponsoredClaimableBalance(sponsorKeyPair, sponsoredKeyPair);
        Console.WriteLine($"  Created sponsored claimable balance: {balanceId}");
        await Task.Delay(2000);

        // Step 2: Transfer sponsorship from sponsorKeyPair → sponsoredKeyPair.
        //
        // The Begin/End block establishes sponsoredKeyPair as the new sponsor.
        // Note the argument order - it looks counterintuitive but is correct:
        //   - BeginSponsoring(sponsoredId: sponsorKeyPair, sourceAccount: sponsoredKeyPair)
        //     means: sponsoredKeyPair (source) will sponsor future reserves for sponsorKeyPair.
        //     This sets up sponsoredKeyPair as the NEW sponsor to receive the transferred entry.
        //   - The revoke operation (submitted by the tx source = sponsorKeyPair, the current
        //     sponsor) releases the claimable balance sponsorship.
        //   - EndSponsoring finalizes the transfer. No sourceAccount is needed because the
        //     tx source (sponsorKeyPair) is the account whose sponsoring relationship ends.
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
        // Both accounts must sign: sponsorKeyPair is the tx source and current sponsor,
        // sponsoredKeyPair agrees to become the new sponsor via BeginSponsoring.
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
    ///     Data entries allow simple revocation - the sponsored account pays its own reserves afterward.
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

        // Simple revoke - no Begin/End block needed for data entries.
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
    ///     Offer entries allow simple revocation — the sponsored account pays its own reserves afterward.
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

        // Simple revoke — no Begin/End block needed for offer entries.
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
    ///     Trustline entries allow simple revocation - the sponsored account pays its own reserves afterward.
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

        // Simple revoke - no Begin/End block needed for trustline entries.
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

    /// <summary>
    ///     Creates a claimable balance where sponsorKeyPair sponsors the reserves for an entry
    ///     created by sponsoredKeyPair. Returns the claimable balance ID.
    /// </summary>
    private static async Task<string> CreateSponsoredClaimableBalance(
        KeyPair sponsorKeyPair,
        KeyPair sponsoredKeyPair)
    {
        var server = new Server(TestNetUrl);
        var account = await server.Accounts.Account(sponsorKeyPair.AccountId);

        var claimants = new[] { new Claimant(sponsorKeyPair, new ClaimPredicateUnconditional()) };
        var createClaimableBalanceOperation =
            new CreateClaimableBalanceOperation(new AssetTypeNative(), "10", claimants, sponsoredKeyPair);

        // sponsorKeyPair (tx source, no explicit sourceAccount) sponsors future reserves
        // for sponsoredKeyPair, covering the claimable balance creation.
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

    /// <summary>
    ///     Creates a data entry on sponsoredKeyPair's account, with sponsorKeyPair paying the reserves.
    /// </summary>
    private static async Task CreateSponsoredData(
        KeyPair sponsorKeyPair,
        KeyPair sponsoredKeyPair)
    {
        var server = new Server(TestNetUrl);
        var account = await server.Accounts.Account(sponsoredKeyPair.AccountId);

        var manageDataOperation = new ManageDataOperation(DataName, "it's a secret");

        // sponsorKeyPair (explicit sourceAccount) sponsors future reserves for sponsoredKeyPair.
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

    /// <summary>
    ///     Creates a sell offer on sponsoredKeyPair's account, with sponsorKeyPair paying the reserves.
    ///     Also sets up a trustline for the asset (unsponsored) before the offer.
    ///     Returns the created offer ID.
    /// </summary>
    private static async Task<long> CreateSponsoredOffer(
        KeyPair sponsorKeyPair,
        KeyPair sponsoredKeyPair,
        Asset asset)
    {
        var server = new Server(TestNetUrl);
        var account = await server.Accounts.Account(sponsoredKeyPair.AccountId);

        // Trustline must exist before placing an offer. This one is NOT sponsored.
        var trustOperation = new ChangeTrustOperation(asset, null, sponsoredKeyPair);

        // Only the offer itself is sponsored by sponsorKeyPair.
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

    /// <summary>
    ///     Creates a trustline on sponsoredKeyPair's account, with sponsorKeyPair paying the reserves.
    /// </summary>
    private static async Task CreateSponsoredTrustline(
        KeyPair sponsorKeyPair,
        KeyPair sponsoredKeyPair,
        Asset asset)
    {
        var server = new Server(TestNetUrl);

        // sponsorKeyPair (explicit sourceAccount) sponsors the trustline reserve for sponsoredKeyPair.
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