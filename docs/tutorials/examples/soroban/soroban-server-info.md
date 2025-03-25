# Connecting to and Querying Soroban Server Information

This guide demonstrates how to connect to the Soroban RPC server and query basic information using the Stellar .NET SDK.

## Understanding Soroban Servers

Soroban is Stellar's smart contract platform. To interact with Soroban, you need to connect to a Soroban RPC server, which provides specialized endpoints for smart contract operations beyond what Horizon offers.

## Setting Up a Soroban Server Connection

To start interacting with Soroban, you first need to define the server URL and create a server instance:

```csharp
// Define the Soroban testnet URL
private const string TestNetSorobanUrl = "https://soroban-testnet.stellar.org";

// Create a new Soroban server instance
SorobanServer server = new(TestNetSorobanUrl);
```

> 📝 **Note**: For development purposes, the Soroban testnet is recommended. In production, you would connect to the mainnet or a private Soroban-enabled network.

## Checking Server Health

Before performing any operations, it's a good practice to check if the Soroban server is operational:

```csharp
private static async Task GetHealth()
{
    SorobanServer server = new(TestNetSorobanUrl);
    var response = await server.GetHealth();
    Console.WriteLine($"Server health: {response.Status}");
}
```

A healthy server will return a status of "healthy". If the server is experiencing issues, you might receive an error or a different status.

## Getting Network Information

You can retrieve information about the Soroban network you're connected to:

```csharp
private static async Task GetNetwork()
{
    SorobanServer server = new(TestNetSorobanUrl);
    var response = await server.GetNetwork();
    Console.WriteLine($"Server passphrase: {response.Passphrase}");
    Console.WriteLine($"Server Friend Bot URL: {response.FriendbotUrl}");
}
```

The network information includes:
- The network passphrase (which identifies the specific Stellar network)
- The FriendBot URL (for testnet only, used to fund test accounts)

## Retrieving Latest Ledger Information

Getting the latest ledger information helps you understand the current state of the blockchain:

```csharp
private static async Task GetLatestLedger()
{
    SorobanServer server = new(TestNetSorobanUrl);
    var response = await server.GetLatestLedger();
    Console.WriteLine($"Server protocol version: {response.ProtocolVersion}");
    Console.WriteLine($"Server latest ledger: {response.Sequence}");
}
```

This provides:
- The current protocol version running on the server
- The sequence number of the latest ledger processed by the server

## Retrieving Account Information

You can get account details from the Soroban server, which is particularly useful when building transactions:

```csharp
// Load account data from the Soroban server
var account = await server.GetAccount(keyPair.AccountId);

// The account object contains sequence number and other details needed for transactions
Console.WriteLine($"Account sequence number: {account.SequenceNumber}");
```

The account information retrieved from Soroban will have the current sequence number needed to build valid transactions.

## Querying Ledger Entries

Soroban provides specific methods to query different types of ledger entries. Here's how to query account data:

```csharp
private static async Task GetLedgerEntryAccount(string accountId)
{
    SorobanServer server = new(TestNetSorobanUrl);

    var ledgerKeyAccount = new LedgerKeyAccount(accountId);
    var ledgerEntriesResponse = await server.GetLedgerEntries([ledgerKeyAccount]);
    var ledgerEntries = ledgerEntriesResponse.LedgerEntries;
    if (ledgerEntries == null || ledgerEntries.Length == 0)
    {
        Console.WriteLine($"Failed to get ledger entry for account {accountId}");
        return;
    }
    var entryAccount = (LedgerEntryAccount)ledgerEntries[0];
    Console.WriteLine($"Entry account ID: {entryAccount.Account.AccountId}");
    Console.WriteLine($"Entry account signing key ID: {entryAccount.Account.SigningKey.AccountId}");
    Console.WriteLine($"Entry account balance: {entryAccount.Balance}");
    
    // Check for extended account data
    var v1 = entryAccount.AccountExtensionV1;
    if (v1 != null)
    {
        Console.WriteLine($"Entry account balance buying liabilities: {v1.Liabilities.Buying}");
        Console.WriteLine($"Entry account balance selling liabilities: {v1.Liabilities.Selling}");
        
        var v2 = v1.ExtensionV2;
        if (v2 != null)
        {
            Console.WriteLine($"Entry account number of sponsored: {v2.NumberSponsored}");
            Console.WriteLine($"Entry account number of sponsoring: {v2.NumberSponsoring}");
        }
    }
}
```

This code retrieves detailed account information from the ledger, including:
- Basic account details (ID, signing key, balance)
- Liabilities information (if available)
- Sponsorship information (if available)

## Querying Claimable Balance Information

You can also query information about claimable balances:

```csharp
private static async Task GetLedgerEntryClaimableBalance(string balanceId)
{
    SorobanServer server = new(TestNetSorobanUrl);

    var ledgerKeyClaimableBalance = new LedgerKeyClaimableBalance(balanceId);
    Console.WriteLine($"Get ledger entry details for claimable balance {balanceId}");
    var ledgerEntriesResponse = await server.GetLedgerEntries([ledgerKeyClaimableBalance]);
    var ledgerEntries = ledgerEntriesResponse.LedgerEntries;
    if (ledgerEntries == null || ledgerEntries.Length == 0)
    {
        Console.WriteLine($"Failed to get ledger entry for claimable balance {balanceId}");
        return;
    }
    var entryClaimableBalance = (LedgerEntryClaimableBalance)ledgerEntries[0];
    Console.WriteLine($"ID: {entryClaimableBalance.BalanceId}");
    Console.WriteLine($"Amount: {entryClaimableBalance.Amount}");
    var claimants = entryClaimableBalance.Claimants;
    Console.WriteLine($"Claimant count: {claimants.Length}");
    for (var i = 0; i < claimants.Length; i++)
    {
        Console.WriteLine($"Claimant {i + 1} address: {claimants[i].Destination.AccountId}");
        Console.WriteLine($"Claimant {i + 1} predicate: {claimants[i].Predicate.GetType()}");
    }
}
```

This retrieves information about a claimable balance, including:
- Balance ID
- Amount
- Claimants and their predicates (conditions under which they can claim)

## Querying Smart Contract Code

Once a smart contract is uploaded to the Soroban network, you can query information about its code:

```csharp
private static async Task GetLedgerEntryContractCode(string contractWasmId)
{
    SorobanServer server = new(TestNetSorobanUrl);

    var ledgerKeyContractCodes = new LedgerKey[]
    {
        new LedgerKeyContractCode(contractWasmId),
    };
    var contractCodeResponse = await server.GetLedgerEntries(ledgerKeyContractCodes);
    var ledgerEntries = contractCodeResponse.LedgerEntries;
    if (ledgerEntries == null || ledgerEntries.Length == 0)
    {
        Console.WriteLine($"Failed to get ledger entries for contractWasmId {contractWasmId}");
        return;
    }
    Console.WriteLine($"Contract code count: {ledgerEntries.Length}");
    var entry = (LedgerEntryContractCode)ledgerEntries[0];
    Console.WriteLine($"Contract code hash in base64: {Convert.ToBase64String(entry.Hash)}");
    Console.WriteLine($"Contract code hash in hex: {Convert.ToHexString(entry.Hash)}");
    Console.WriteLine($"Contract code TTL: {entry.LiveUntilLedger}");
}
```

This code retrieves:
- The contract's code hash in different formats
- The contract's time-to-live (TTL) expressed as a ledger sequence number

## Querying Smart Contract Data

You can also query data associated with a deployed smart contract:

```csharp
private static async Task<(uint ledgerSeq, uint ttl)> GetLedgerEntryContractData(string contractId)
{
    SorobanServer server = new(TestNetSorobanUrl);

    var ledgerKeyContractData = CreateLedgerKeyContractData(contractId);
    var contractCodeResponse = await server.GetLedgerEntries([ledgerKeyContractData]);
    var ledgerEntries = contractCodeResponse.LedgerEntries;
    if (ledgerEntries == null || ledgerEntries.Length == 0)
    {
        Console.WriteLine($"Failed to get ledger entries for contract ID {contractId}");
    }
    Console.WriteLine($"Contract data count: {ledgerEntries!.Length}");
    var entry = (LedgerEntryContractData)ledgerEntries[0];
    var ledgerSeq = entry.LastModifiedLedgerSeq;
    var ttl = entry.LiveUntilLedger;
    ArgumentNullException.ThrowIfNull(ttl);
    Console.WriteLine($"Contract data ledger sequence: {ledgerSeq}");
    Console.WriteLine($"Contract data TTL: {ttl}");
    return (ledgerSeq, ttl.Value);
}

// Helper method to create a contract data ledger key
private static LedgerKey CreateLedgerKeyContractData(string contractId)
{
    var scContractId = new SCContractId(contractId);

    var contractDataDurability =
        ContractDataDurability.Create(ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT);
    var ledgerKeyContractData = new LedgerKeyContractData(
        scContractId,
        new SCLedgerKeyContractInstance(),
        contractDataDurability
    );
    return ledgerKeyContractData;
}
```

This retrieves contract instance data, including:
- The ledger sequence when it was last modified
- The contract's time-to-live (when it will expire if not extended)

## Polling for Transaction Results

When submitting transactions to Soroban, you may need to continuously check for transaction completion:

```csharp
// Keep querying for the transaction using `GetTransaction` endpoint until success or error
private static async Task<GetTransactionResponse> PollTransaction(string transactionHash)
{
    SorobanServer server = new(TestNetSorobanUrl);
    var status = TransactionInfo.TransactionStatus.NOT_FOUND;
    GetTransactionResponse? transactionResponse = null;
    while (status == TransactionInfo.TransactionStatus.NOT_FOUND)
    {
        await Task.Delay(3000);
        Console.WriteLine($"Fetching details for transaction {transactionHash}");
        transactionResponse = await server.GetTransaction(transactionHash);

        status = transactionResponse.Status;
        if (status == TransactionInfo.TransactionStatus.FAILED)
        {
            Console.WriteLine($"Transaction {transactionHash} failed");
            break;
        }
        if (status == TransactionInfo.TransactionStatus.SUCCESS)
        {
            ArgumentNullException.ThrowIfNull(transactionResponse.ResultXdr);
            Console.WriteLine($"Transaction {transactionHash} was successful");
            return transactionResponse;
        }
        Console.WriteLine("Transaction not updated on-chain. Waiting for a moment before retrying..");
    }

    ArgumentNullException.ThrowIfNull(transactionResponse);
    return transactionResponse;
}
```

This function:
- Checks the transaction status every few seconds
- Waits until the transaction succeeds or fails
- Returns the final transaction response

## Additional Resources

- [Soroban Documentation](https://soroban.stellar.org/docs)
- [Soroban RPC API Reference](https://soroban.stellar.org/api/methods)
- [Stellar .NET SDK Documentation](https://elucidsoft.github.io/dotnet-stellar-sdk/)
- [Stellar Developers Guide](https://developers.stellar.org/docs)
