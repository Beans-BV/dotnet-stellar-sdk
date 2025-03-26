﻿using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Responses.Operations;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Examples.Horizon;

public static class HorizonExamples
{
    private const string TestNetUrl = "https://horizon-testnet.stellar.org";

    public static async Task Main(string[] args)
    {
        Network.UseTestNetwork();

        Console.WriteLine("Create a key pair");
        var keyPair = CreateKeyPair();
        Console.WriteLine($"\nFund account {keyPair.AccountId} using Friend Bot");
        await FundAccountUsingFriendBot(keyPair.AccountId);

        Console.WriteLine("\nGet account balances");
        await GetAccountBalances(keyPair.AccountId);

        await CreateChildAccountWithPositiveStartingBalance(keyPair, "0.5");
        
        Console.WriteLine("\nPrepare to send native assets");
        Console.WriteLine("Create new child account to receive the native assets");
        var (childKeyPair, txHash) = await CreateChildAccountWithSponsorship(keyPair);
        Console.WriteLine($"Get child account {childKeyPair.AccountId} balances");
        await GetAccountBalances(childKeyPair.AccountId);
        Console.WriteLine("Send native assets");
        await SendNativeAssets(sourceKeypair: keyPair, destinationAccountId: childKeyPair.AccountId);
        Console.WriteLine("Get child account balances after receiving the assets");
        await GetAccountBalances(childKeyPair.AccountId);
        Console.WriteLine("Get parent account balances after sending the assets");
        await GetAccountBalances(keyPair.AccountId);
        Console.WriteLine("\nGet create account transaction details");
        await GetTransaction(txHash);
        Console.WriteLine("\nGet create account transaction operation details");
        await GetTransactionOperations(txHash);

        Console.WriteLine("\nPrepare to send non-native assets");
        Console.WriteLine("Create new child issuer account with positive starting balance");
        var (issuerKeyPair, _) = await CreateChildAccountWithPositiveStartingBalance(keyPair);
        var assetCode = "USDT";
        Console.WriteLine(
            $"Parent sponsors ChangeTrustAssetOperation to {assetCode}:{issuerKeyPair.AccountId} for child account {childKeyPair.AccountId}");
        await TrustAssetWithSponsorship(keyPair, childKeyPair, issuerKeyPair.AccountId, assetCode);
        Console.WriteLine(
            $"Issuer {issuerKeyPair.AccountId} sends {assetCode} assets to child account {childKeyPair.AccountId}");
        await SendNonNativeAssets(issuerKeyPair, childKeyPair.AccountId, assetCode);

        Console.WriteLine("\nPrepare to send non-native assets with fee bump transaction");
        Console.WriteLine("Create new child issuer account with zero starting balance");
        var (anotherIssuerKeyPair, _) = await CreateChildAccountWithSponsorship(keyPair);
        Console.WriteLine(
            $"Parent sponsors ChangeTrustAssetOperation to {assetCode}:{anotherIssuerKeyPair.AccountId} for child account {childKeyPair.AccountId}");
        await TrustAssetWithSponsorship(keyPair, childKeyPair, anotherIssuerKeyPair.AccountId, assetCode);
        Console.WriteLine(
            $"Issuer {anotherIssuerKeyPair.AccountId} sends {assetCode} assets to child account {childKeyPair.AccountId}");
        await SendNonNativeAssetsWithFeeBump(keyPair, anotherIssuerKeyPair, childKeyPair.AccountId, assetCode);
    }

    public static KeyPair CreateKeyPair()
    {
        // Create a completely new and unique pair of keys.
        var keyPair = KeyPair.Random();

        // Print the public address and secret seed
        Console.WriteLine("Account ID: " + keyPair.AccountId);
        Console.WriteLine("Secret: " + keyPair.SecretSeed);
        return keyPair;
    }

    public static async Task FundAccountUsingFriendBot(string accountId)
    {
        var server = new Server(TestNetUrl);

        // Get 10,000 test XLM from FriendBot
        await server.TestNetFriendBot.FundAccount(accountId).Execute();

        Console.WriteLine("SUCCESS! You have a new account :)");
    }

    public static async Task GetAccountBalances(string accountId)
    {
        var server = new Server(TestNetUrl);

        // Load the account
        var accountResponse = await server.Accounts.Account(accountId);

        // Get the balances
        var balances = accountResponse.Balances;

        Console.WriteLine($"Balances of account ID {accountId}:");
        // Show the balance
        foreach (var balance in balances)
        {
            Console.WriteLine("Asset: " + balance.Asset?.CanonicalName());
            Console.WriteLine("Asset amount: " + balance.BalanceString);
        }
    }

    public static async Task TrustAssetWithSponsorship(
        KeyPair parentAccountKeyPair,
        KeyPair childAccountKeyPair,
        string issuerAccountId,
        string assetCode)
    {
        // Create server
        var server = new Server(TestNetUrl);

        // Load the account
        var accountResponse = await server.Accounts.Account(parentAccountKeyPair.AccountId);

        var asset = Asset.CreateNonNativeAsset(assetCode, issuerAccountId);

        var beginSponsoringOperation = new BeginSponsoringFutureReservesOperation(childAccountKeyPair);
        var operation = new ChangeTrustOperation(asset, ChangeTrustOperation.MaxLimit, childAccountKeyPair);
        var endSponsoringOperation = new EndSponsoringFutureReservesOperation(childAccountKeyPair);

        // Create transaction and add the operations
        var transaction = new TransactionBuilder(accountResponse)
            .AddOperation(beginSponsoringOperation)
            .AddOperation(operation)
            .AddOperation(endSponsoringOperation)
            .Build();

        transaction.Sign(parentAccountKeyPair);
        transaction.Sign(childAccountKeyPair);

        await SubmitTransaction(transaction);
    }

    public static async Task<(KeyPair, string)> CreateChildAccountWithPositiveStartingBalance(
        KeyPair parentAccountKeyPair,
        string startingBalance = "10")
    {
        // Create server
        var server = new Server(TestNetUrl);

        var childKeyPair = CreateKeyPair();
        // Load the account
        var accountResponse = await server.Accounts.Account(parentAccountKeyPair.AccountId);

        // Create a create account operation
        var operation = new CreateAccountOperation(childKeyPair, startingBalance);

        // Create transaction and add the operation
        var transaction = new TransactionBuilder(accountResponse).AddOperation(operation).Build();

        transaction.Sign(parentAccountKeyPair);

        var txHash = await SubmitTransaction(transaction);

        return (childKeyPair, txHash);
    }

    public static async Task<(KeyPair, string)> CreateChildAccountWithSponsorship(KeyPair parentAccountKeyPair)
    {
        // Create server
        var server = new Server(TestNetUrl);

        var childKeyPair = CreateKeyPair();
        // Load the account
        var accountResponse = await server.Accounts.Account(parentAccountKeyPair.AccountId);

        // The child account must be sponsored as a basic reserve is required 
        var beginSponsoringOperation = new BeginSponsoringFutureReservesOperation(childKeyPair.AccountId);

        // Create a create account operation
        var operation = new CreateAccountOperation(childKeyPair, "0");

        var endSponsoringOperation = new EndSponsoringFutureReservesOperation(childKeyPair);
        // Create transaction and add the operation
        var transaction = new TransactionBuilder(accountResponse)
            .AddOperation(beginSponsoringOperation)
            .AddOperation(operation)
            .AddOperation(endSponsoringOperation)
            .Build();

        transaction.Sign(parentAccountKeyPair);
        transaction.Sign(childKeyPair);

        var txHash = await SubmitTransaction(transaction);

        return (childKeyPair, txHash);
    }

    public static async Task<string> SubmitTransaction(Transaction transaction)
    {
        // Create server
        var server = new Server(TestNetUrl);
        var txHash = "";
        // Submit the transaction
        try
        {
            Console.WriteLine($"Submitting transaction {transaction.ToEnvelopeXdrBase64()}");
            var response = await server.SubmitTransaction(transaction);
            if (response is not { IsSuccess: true })
            {
                Console.WriteLine("Fail!");
                Console.WriteLine($"Result XDR: {response?.ResultXdr}");
                return null;
            }
            Console.WriteLine("Success!");
            txHash = response.Hash;
            Console.WriteLine($"Transaction hash: {txHash}");
        }
        catch (Exception exception)
        {
            Console.WriteLine("Failed to submit transaction");
            Console.WriteLine("Exception: " + exception.Message);
        }
        return txHash;
    }

    public static async Task SendNativeAssets(KeyPair sourceKeypair, string destinationAccountId)
    {
        // Create server
        var server = new Server(TestNetUrl);

        // Load source account data with the latest sequence number
        var sourceAccountResponse = await server.Accounts.Account(sourceKeypair.AccountId);

        // Create key pair from account ID
        var destinationKeyPair = KeyPair.FromAccountId(destinationAccountId);
        // Create asset object with specific amount
        // You can use native or non-native ones.
        Asset asset = new AssetTypeNative();
        var amount = "1";

        // Create payment operation
        var operation = new PaymentOperation(destinationKeyPair, asset, amount, sourceKeypair);

        // Create transaction and add the payment operation
        var transaction = new TransactionBuilder(sourceAccountResponse).AddOperation(operation).Build();

        // Sign the transaction with the source key pair
        // The source key pair must contain the secret key
        transaction.Sign(sourceKeypair);

        // Submit the transaction
        await SubmitTransaction(transaction);
    }

    public static async Task<string> FeeBumpTransaction(
        KeyPair sponsorKeyPair,
        Transaction transaction)
    {
        // Create server
        var server = new Server(TestNetUrl);

        var feeBumpTransaction = TransactionBuilder.BuildFeeBumpTransaction(
            sponsorKeyPair,
            transaction,
            2000000
        );
        feeBumpTransaction.Sign(sponsorKeyPair);

        var response = await server.SubmitTransaction(feeBumpTransaction);

        if (response?.IsSuccess ?? false)
        {
            Console.WriteLine($"Fee bump transaction {response.Hash} was successful");
            return response.Hash;
        }
        throw new Exception("Fee bump transaction failed");
    }

    public static async Task<string> SendNonNativeAssetsWithFeeBump(
        KeyPair sponsorKeypair,
        KeyPair issuerKeyPair,
        string destinationAccountId,
        string assetCode)
    {
        // Create server
        var server = new Server(TestNetUrl);

        // Load source account data with the latest sequence number
        var accountResponse = await server.Accounts.Account(issuerKeyPair.AccountId);

        // Create key pair from account ID
        var destinationKeyPair = KeyPair.FromAccountId(destinationAccountId);
        // Create asset object with specific amount
        // You can use native or non-native ones.
        Asset asset = Asset.CreateNonNativeAsset(assetCode, issuerKeyPair.AccountId);
        var amount = "200";

        // Create payment operation
        var operation = new PaymentOperation(destinationKeyPair, asset, amount, issuerKeyPair);

        // Create transaction and add the payment operation
        var transaction = new TransactionBuilder(accountResponse).AddOperation(operation).Build();

        transaction.Sign(issuerKeyPair);

        var feeBumpTxHash = await FeeBumpTransaction(sponsorKeypair, transaction);
        return feeBumpTxHash;
    }

    public static async Task SendNonNativeAssets(
        KeyPair issuerKeyPair,
        string destinationAccountId,
        string assetCode)
    {
        // Create server
        var server = new Server(TestNetUrl);

        // Load source account data with the latest sequence number
        var accountResponse = await server.Accounts.Account(issuerKeyPair.AccountId);

        // Create key pair from account ID
        var destinationKeyPair = KeyPair.FromAccountId(destinationAccountId);
        // Create asset object with specific amount
        // You can use native or non-native ones.
        Asset asset = Asset.CreateNonNativeAsset(assetCode, issuerKeyPair.AccountId);
        var amount = "100";

        // Create payment operation
        var operation = new PaymentOperation(destinationKeyPair, asset, amount, issuerKeyPair);

        // Create transaction and add the payment operation
        var transaction = new TransactionBuilder(accountResponse).AddOperation(operation).Build();

        // Sign the transaction with the source key pair
        // The source key pair must contain the secret key
        transaction.Sign(issuerKeyPair);

        // Submit the transaction
        await SubmitTransaction(transaction);
    }

    private static async Task GetTransaction(string txHash)
    {
        var server = new Server(TestNetUrl);
        // Get Transaction details from Horizon testnet
        var transactionResponse = await server.Transactions.Transaction(txHash);

        Console.WriteLine($"Operation count: {transactionResponse.OperationCount}");
        Console.WriteLine($"Transaction envelope xdr: {transactionResponse.EnvelopeXdr}");
    }

    private static async Task<List<OperationResponse>> GetTransactionOperations(string txHash)
    {
        var server = new Server(TestNetUrl);
        // Get transaction operation details from Horizon testnet
        var operations = await server.Operations.ForTransaction(txHash).Execute();
        Console.WriteLine($"Operation count: {operations.Records.Count}");
        Console.WriteLine($"Link to operations: {operations.Links}");

        return operations.Records;
    }
}