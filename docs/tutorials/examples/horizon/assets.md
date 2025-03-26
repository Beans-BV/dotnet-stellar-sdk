# Asset Management

This guide demonstrates how to work with assets using the Stellar .NET SDK.

## Understanding Stellar Assets

Stellar supports two types of assets:

1. **Native Asset (XLM)**: The built-in asset of the Stellar network.
2. **Custom Assets**: Assets issued by accounts on the network.

Each custom asset on Stellar has:
- An **asset code** (up to 12 alphanumeric characters)
- An **issuer** (the account that created the asset)

## Creating Asset Objects

Before you can send or receive assets, you need to create the appropriate asset objects.

### Native Asset (XLM)

```csharp
// Create a native asset (XLM) object
Asset nativeAsset = new AssetTypeNative();
```

### Custom (Non-Native) Asset

```csharp
// Create a non-native asset with a code and issuer account ID
Asset customAsset = Asset.CreateNonNativeAsset("USDT", issuerAccountId);
```

## Establishing Trust for an Asset

Before an account can hold a custom asset, it must establish a trust line to that asset.

```csharp
// Create server connection
var server = new Server(TestNetUrl);

// Load the account
var accountResponse = await server.Accounts.Account(parentAccountKeyPair.AccountId);

// Create the custom asset
var asset = Asset.CreateNonNativeAsset(assetCode, issuerAccountId);

// Begin sponsoring future reserves for the child account
var beginSponsoringOperation = new BeginSponsoringFutureReservesOperation(childAccountKeyPair);

// Create ChangeTrust operation - the second parameter "ChangeTrustOperation.MaxLimit" allows holding
// the maximum possible amount of the asset
var operation = new ChangeTrustOperation(asset, ChangeTrustOperation.MaxLimit, childAccountKeyPair);

// End the sponsorship
var endSponsoringOperation = new EndSponsoringFutureReservesOperation(childAccountKeyPair);

// Create transaction and add the operations
var transaction = new TransactionBuilder(accountResponse)
    .AddOperation(beginSponsoringOperation)
    .AddOperation(operation)
    .AddOperation(endSponsoringOperation)
    .Build();

// Both accounts must sign the transaction
transaction.Sign(parentAccountKeyPair);
transaction.Sign(childAccountKeyPair);

// Submit the transaction
await SubmitTransaction(transaction);
```

In this example, a parent account is sponsoring the reserve requirements for establishing a trust line for a child account.

## Sending Native Assets (XLM)

To send XLM from one account to another:

```csharp
// Create server connection
var server = new Server(TestNetUrl);

// Load source account data with the latest sequence number
var sourceAccountResponse = await server.Accounts.Account(sourceKeypair.AccountId);

// Create key pair from destination account ID
var destinationKeyPair = KeyPair.FromAccountId(destinationAccountId);

// Use the native asset (XLM)
Asset asset = new AssetTypeNative();
var amount = "1";  // Amount in XLM

// Create payment operation
var operation = new PaymentOperation(destinationKeyPair, asset, amount, sourceKeypair);

// Create transaction and add the payment operation
var transaction = new TransactionBuilder(sourceAccountResponse).AddOperation(operation).Build();

// Sign the transaction with the source key pair
transaction.Sign(sourceKeypair);

// Submit the transaction
await SubmitTransaction(transaction);
```

## Sending Custom Assets

To send a custom asset, the destination account must first establish a trust line to the asset issuer.

```csharp
// Create server connection
var server = new Server(TestNetUrl);

// Load source account data with the latest sequence number
var accountResponse = await server.Accounts.Account(issuerKeyPair.AccountId);

// Create key pair from destination account ID
var destinationKeyPair = KeyPair.FromAccountId(destinationAccountId);

// Create the custom asset
Asset asset = Asset.CreateNonNativeAsset(assetCode, issuerKeyPair.AccountId);
var amount = "100";

// Create payment operation
var operation = new PaymentOperation(destinationKeyPair, asset, amount, issuerKeyPair);

// Create transaction and add the payment operation
var transaction = new TransactionBuilder(accountResponse).AddOperation(operation).Build();

// Sign the transaction with the issuer key pair
transaction.Sign(issuerKeyPair);

// Submit the transaction
await SubmitTransaction(transaction);
```

## Sending Assets with Fee Bump Transactions

Fee bump transactions allow one account to pay the fee for another account's transaction, which is especially useful for asset issuers or service providers.

```csharp
// Create server connection
var server = new Server(TestNetUrl);

// Load source account data with the latest sequence number
var accountResponse = await server.Accounts.Account(issuerKeyPair.AccountId);

// Create key pair from destination account ID
var destinationKeyPair = KeyPair.FromAccountId(destinationAccountId);

// Create the custom asset
Asset asset = Asset.CreateNonNativeAsset(assetCode, issuerKeyPair.AccountId);
var amount = "200";

// Create payment operation
var operation = new PaymentOperation(destinationKeyPair, asset, amount, issuerKeyPair);

// Create transaction and add the payment operation
var transaction = new TransactionBuilder(accountResponse).AddOperation(operation).Build();

// Sign the transaction with the issuer key pair
transaction.Sign(issuerKeyPair);

// Create a fee bump transaction where the sponsor pays the fee
var feeBumpTransaction = TransactionBuilder.BuildFeeBumpTransaction(
    sponsorKeypair,
    transaction,
    2000000  // Fee in stroops (1 XLM = 10,000,000 stroops)
);
feeBumpTransaction.Sign(sponsorKeypair);

// Submit the fee bump transaction
var response = await server.SubmitTransaction(feeBumpTransaction);
```

## Checking Balances for Specific Assets

After sending or receiving assets, you can check an account's balances to verify the transaction:

```csharp
// Create server connection
var server = new Server(TestNetUrl);

// Load the account
var accountResponse = await server.Accounts.Account(accountId);

// Get all balances for the account
var balances = accountResponse.Balances;

// Display balance information for all assets
foreach (var balance in balances)
{
    Console.WriteLine("Asset: " + balance.Asset?.CanonicalName());
    Console.WriteLine("Asset amount: " + balance.BalanceString);
}

// Find balance for a specific asset
var specificAssetBalance = balances.FirstOrDefault(b => 
    b.AssetType != "native" && 
    b.AssetCode == assetCode && 
    b.AssetIssuer == issuerAccountId);

if (specificAssetBalance != null)
{
    Console.WriteLine($"Balance for {assetCode}: {specificAssetBalance.BalanceString}");
}
```

## Creating Claimable Balances

Claimable balances allow you to send assets to an account that doesn't yet exist or doesn't have a trust line established.

```csharp
// Create server connection
var server = new Server(TestNetUrl);

// Create a claimant who can claim the balance
var claimant = new Claimant(claimantAccount.AccountId, new ClaimPredicateUnconditional());

// Create the claimable balance operation with the native asset
var operation = new CreateClaimableBalanceOperation(
    new AssetTypeNative(),  // Use native XLM asset
    "100",  // Amount
    [claimant]  // Array of claimants who can claim this balance
);

// Load the account
var account = await server.Accounts.Account(keyPair.AccountId);

// Build and sign the transaction
var tx = new TransactionBuilder(account).AddOperation(operation).Build();
tx.Sign(keyPair);

// Submit the transaction
var txResponse = await SubmitTransaction(tx);

// Extract the balance ID from the transaction result
var resultXdr = txResponse.ResultXdr;
var transactionResult = TransactionResult.FromXdrBase64(resultXdr);
var results = ((TransactionResultSuccess)transactionResult).Results;
var operationResult = (CreateClaimableBalanceSuccess)results.First();
var balanceId = operationResult.BalanceId;

Console.WriteLine($"Created claimable balance with ID: {balanceId}");
```

## Additional Resources

- [Stellar Assets Concepts](https://developers.stellar.org/docs/learn/fundamentals/stellar-data-structures/assets)
- [Asset Issuance Guide](https://developers.stellar.org/docs/tokens/how-to-issue-an-asset)
- [Claimable Balances](https://developers.stellar.org/docs/learn/encyclopedia/claimable-balances)
