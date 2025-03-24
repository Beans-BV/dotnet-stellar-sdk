# Account Management

This guide demonstrates how to perform account management operations using the Stellar .NET SDK.

## Creating a Key Pair

Before interacting with the Stellar network, you need to create a key pair. A key pair consists of a public key (account ID) and a private key (secret seed).

```csharp
// Create a completely new and unique pair of keys
var keyPair = KeyPair.Random();

// The public key is used as the account ID
Console.WriteLine("Account ID: " + keyPair.AccountId);

// The secret seed should be kept secure and never shared
Console.WriteLine("Secret: " + keyPair.SecretSeed);
```

> ⚠️ **Security Warning**: Never hardcode secret seeds in your application code or store them in plain text. In production, use secure storage solutions and ensure secrets are properly encrypted at rest.

## Funding an Account Using FriendBot (Testnet Only)

New accounts on Stellar need to be funded with an initial balance. On the testnet, you can use FriendBot to fund your account for testing.

```csharp
// Define the testnet URL
private const string TestNetUrl = "https://horizon-testnet.stellar.org";

// Use the FriendBot to get test funds (10,000 XLM)
var server = new Server(TestNetUrl);
await server.TestNetFriendBot.FundAccount(keyPair.AccountId).Execute();
```

> 📝 **Note**: FriendBot is only available on the testnet and should never be relied upon for production applications.

## Checking Account Balances

Once an account is created and funded, you can query its balances to see available assets:

```csharp
// Create server connection
var server = new Server(TestNetUrl);

// Load the account information
var accountResponse = await server.Accounts.Account(accountId);

// Get all balances for the account
var balances = accountResponse.Balances;

// Display balance information
foreach (var balance in balances)
{
    Console.WriteLine("Asset: " + balance.Asset?.CanonicalName());
    Console.WriteLine("Asset amount: " + balance.BalanceString);
}
```

This will return all assets held by the account, including native XLM and any other assets the account has trust lines for.

## Creating a Child Account with Starting Balance

To create a new account on the Stellar network, an existing account must fund it with the minimum reserve:

```csharp
// Create server connection
var server = new Server(TestNetUrl);

// Generate new key pair for the child account
var childKeyPair = KeyPair.Random();

// Load the parent account to get the current sequence number
var accountResponse = await server.Accounts.Account(parentAccountKeyPair.AccountId);

// Create a "create account" operation with a starting balance
var operation = new CreateAccountOperation(childKeyPair, "10");  // 10 XLM starting balance

// Build and submit the transaction
var transaction = new TransactionBuilder(accountResponse)
    .AddOperation(operation)
    .Build();

transaction.Sign(parentAccountKeyPair);
var response = await SubmitTransaction(transaction);
```

The minimum balance for a new account is 1 XLM, but it's recommended to provide additional XLM to allow the new account to perform operations.

## Creating Child Accounts with Sponsorship

Sponsorship allows one account to pay the reserves for another account. This is useful when creating accounts that might not immediately have the funds for the base reserve:

```csharp
// Create server connection
var server = new Server(TestNetUrl);

// Generate new key pair for the child account
var childKeyPair = KeyPair.Random();

// Load the parent account
var accountResponse = await server.Accounts.Account(parentAccountKeyPair.AccountId);

// Begin sponsoring future reserves
var beginSponsoringOperation = new BeginSponsoringFutureReservesOperation(childKeyPair.AccountId);

// Create the account with zero balance
var createAccountOperation = new CreateAccountOperation(childKeyPair, "0");

// End the sponsorship
var endSponsoringOperation = new EndSponsoringFutureReservesOperation(childKeyPair);

// Build the transaction with all three operations
var transaction = new TransactionBuilder(accountResponse)
    .AddOperation(beginSponsoringOperation)
    .AddOperation(createAccountOperation)
    .AddOperation(endSponsoringOperation)
    .Build();

// Both accounts must sign: the sponsor and the sponsored
transaction.Sign(parentAccountKeyPair);
transaction.Sign(childKeyPair);

// Submit the transaction
var response = await SubmitTransaction(transaction);
```

This technique is particularly useful when creating accounts for users without requiring them to purchase XLM first.

## Additional Resources

- [Stellar Account Concepts](https://developers.stellar.org/docs/fundamentals-and-concepts/stellar-network-overview/accounts)
- [Reserve Requirements](https://developers.stellar.org/docs/fundamentals-and-concepts/stellar-network-overview/minimum-balance)
- [Sponsorship Documentation](https://developers.stellar.org/docs/encyclopedia/sponsored-reserves)
