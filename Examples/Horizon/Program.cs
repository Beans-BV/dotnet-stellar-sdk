using StellarDotnetSdk.Accounts;
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
        
        Console.WriteLine("Creating a key pair");
        var keyPair = CreateKeyPair();
        Console.WriteLine($"\nFunding account {keyPair.AccountId} using Friend Bot");
        await FundAccountUsingFriendBot(keyPair.AccountId);

        Console.WriteLine("\nGetting account balances");
        await GetAccountBalances(keyPair.AccountId);

        Console.WriteLine("\nPreparing to send native assets");
        Console.WriteLine("Creating new child account to receive the assets");
        var (childKeyPair, txHash) = await CreateAccount(keyPair);
        Console.WriteLine("\nGetting child account balances");
        await GetAccountBalances(childKeyPair.AccountId);
        Console.WriteLine("Sending native assets");
        await SendNativeAssets(sourceKeypair: keyPair, destinationAccountId: childKeyPair.AccountId);
        Console.WriteLine("\nGetting child account balances after receiving the assets");
        await GetAccountBalances(childKeyPair.AccountId);
        Console.WriteLine("\nGetting parent account balances after sending the assets");
        await GetAccountBalances(keyPair.AccountId);
        Console.WriteLine("\nGetting create account transaction details");
        await GetTransaction(txHash);
        Console.WriteLine("\nGetting create account transaction operation details");
        await GetTransactionOperations(txHash);
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

    public static async Task<(KeyPair, string)> CreateAccount(KeyPair parentAccountKeyPair)
    {
        // Create server
        var server = new Server(TestNetUrl);
        
        var childKeyPair = CreateKeyPair();
        var childAccountInitialXlmBalance = "0";
        // Load the account
        var accountResponse = await server.Accounts.Account(parentAccountKeyPair.AccountId);

        // Create a create account operation
        var operation = new CreateAccountOperation(childKeyPair, childAccountInitialXlmBalance);

        // Create transaction and add the operation
        var transaction = new TransactionBuilder(accountResponse).AddOperation(operation).Build();

        transaction.Sign(parentAccountKeyPair);

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
            Console.WriteLine("Submitting transaction");
            Console.WriteLine($"Transaction envelope xdr: {transaction.ToEnvelopeXdrBase64()}");
            var response = await server.SubmitTransaction(transaction);
            if (response!.IsSuccess)
            {
                Console.WriteLine("Fail!");
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