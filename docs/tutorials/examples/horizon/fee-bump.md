# Fee Bump Transactions

This guide demonstrates how to create and use fee bump transactions with the Stellar .NET SDK.

## Understanding Fee Bump Transactions

Fee bump transactions are a powerful feature of the Stellar network that allows one account to pay the transaction fee for another account. This mechanism is useful in several scenarios:

- Service providers can pay fees on behalf of their users
- Asset issuers can cover fees for transactions involving their asset
- Applications can ensure transactions succeed even if the user's account has limited XLM

Fee bump transactions wrap an existing transaction (called the "inner transaction") with a new envelope that specifies a different fee-paying account.

## Creating a Basic Fee Bump Transaction

To create a fee bump transaction, you first need to create a regular transaction, then wrap it with a fee bump:

```csharp
// Create the inner transaction
var server = new Server(TestNetUrl);
var innerAccount = await server.Accounts.Account(userKeyPair.AccountId);
var innerOperation = new PaymentOperation(destinationKeyPair, asset, amount, userKeyPair);
var innerTransaction = new TransactionBuilder(innerAccount).AddOperation(innerOperation).Build();
innerTransaction.Sign(userKeyPair);

// Create the fee bump transaction
var feeBumpTransaction = TransactionBuilder.BuildFeeBumpTransaction(
    sponsorKeyPair,  // The account that will pay the fee
    innerTransaction, // The transaction to wrap
    2000000          // The fee in stroops (1 XLM = 10,000,000 stroops)
);

// The fee payer needs to sign the fee bump transaction
feeBumpTransaction.Sign(sponsorKeyPair);

// Submit the fee bump transaction
var response = await server.SubmitTransaction(feeBumpTransaction);
```

In this example, `userKeyPair` is the original transaction creator, and `sponsorKeyPair` is the account that will pay the transaction fee.

> ⚠️ **Important**: The inner transaction must still be properly signed by its source account, even though the fee will be paid by a different account.

## Real-World Example: Sponsoring Asset Transfers

One common use case for fee bump transactions is when an asset issuer wants to enable users to send their asset without requiring them to have XLM for fees. Here's a complete example from the SDK:

```csharp
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
```

In this example, the issuer account (`issuerKeyPair`) is sending assets to a destination account, but the transaction fee is paid by a sponsor account (`sponsorKeyPair`).

## Setting Appropriate Fee Amounts

When using fee bump transactions, you should set an appropriate fee. Here are some guidelines:

```csharp
// For standard operations (like payments, create account, etc.)
var standardFee = 100;

// For transactions with multiple operations
var multiOpFee = baseOperationFee * numberOfOperations;
```

When setting the fee for a fee bump transaction, remember:

1. The fee must be at least the minimum fee required by the network
2. During network congestion, you may need to increase fees to ensure priority processing

> 📝 **Note**: Fees are specified in stroops, where 1 XLM = 10,000,000 stroops.

## Verifying Fee Bump Transactions

After submitting a fee bump transaction, you can verify the details:

```csharp
private static async Task GetTransaction(string txHash)
{
    var server = new Server(TestNetUrl);
    // Get Transaction details from Horizon testnet
    var transactionResponse = await server.Transactions.Transaction(txHash);

    // Check if it's a fee bump transaction
    if (transactionResponse.FeeAccount != null)
    {
        Console.WriteLine("This is a fee bump transaction");
        Console.WriteLine($"Fee paying account: {transactionResponse.FeeAccount}");
        Console.WriteLine($"Fee charged: {transactionResponse.FeeCharged}");
        Console.WriteLine($"Inner transaction hash: {transactionResponse.InnerTransactionHash}");
    }

    Console.WriteLine($"Operation count: {transactionResponse.OperationCount}");
    Console.WriteLine($"Transaction envelope xdr: {transactionResponse.EnvelopeXdr}");
}
```

This code fetches transaction details and checks if it's a fee bump transaction by looking for the presence of a fee account.

## Additional Resources

- [Stellar Fee Bump Transactions](https://developers.stellar.org/docs/learn/encyclopedia/transactions-specialized/fee-bump-transactions)
- [Fees overview](https://developers.stellar.org/docs/learn/fundamentals/fees-resource-limits-metering#fees-overview)
