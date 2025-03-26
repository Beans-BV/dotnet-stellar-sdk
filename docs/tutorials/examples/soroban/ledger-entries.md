# Querying Different Types of Ledger Entries

This guide demonstrates how to query different types of ledger entries using the Stellar .NET SDK.

## Understanding Stellar Ledger Entries

The Stellar ledger stores different types of data entries that represent various objects in the Stellar network:

- **Account**: Information about accounts on the network
- **Trustline**: Trust relationships between accounts and assets
- **Offer**: Asset exchange offers made on the decentralized exchange
- **Claimable Balance**: Funds that can be claimed by specific accounts
- **Contract Code**: Smart contract code uploaded to the Soroban platform
- **Contract Data**: Data associated with deployed smart contracts

The Soroban RPC server provides specialized endpoints to query these ledger entries directly.

## Setting Up the Connection

First, establish a connection to the Soroban RPC server:

```csharp
// Define the Soroban testnet URL
private const string TestNetSorobanUrl = "https://soroban-testnet.stellar.org";

// Create a new Soroban server instance
SorobanServer server = new(TestNetSorobanUrl);
```

> 📝 **Note**: For Horizon-related ledger entries, you can also use a Horizon server connection, but Soroban RPC provides more comprehensive ledger entry access.

## Querying Account Entries

Account entries contain fundamental information about accounts on the Stellar network, including balances, signers, and more:

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
    var v1 = entryAccount.AccountExtensionV1;
    if (v1 == null)
    {
        return;
    }

    Console.WriteLine($"Entry account balance buying liabilities: {v1.Liabilities.Buying}");
    Console.WriteLine($"Entry account balance selling liabilities: {v1.Liabilities.Selling}");
    var v2 = v1.ExtensionV2;
    if (v2 == null)
    {
        return;
    }
    Console.WriteLine($"Entry account number of sponsored: {v2.NumberSponsored}");
    Console.WriteLine($"Entry account number of sponsoring: {v2.NumberSponsoring}");
}
```

The account entry provides detailed information including:
- The account ID and signing key
- The account's XLM balance
- Buying and selling liabilities (used for offers)
- Sponsorship information

## Querying Claimable Balance Entries

Claimable balances are funds set aside for specific claimants to claim under certain conditions:

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
- The unique balance ID
- The amount of the asset available to claim
- The list of claimants and their claim predicates (conditions)

> ⚠️ **Important**: The balance ID format from transaction results may need to be normalized. In some cases, you might need to prepend "00000000" to the balance ID as shown in the example code.

## Querying Smart Contract Code Entries

Contract code entries contain the WebAssembly (WASM) code of uploaded smart contracts:

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

This provides information about the uploaded contract code, including:
- The contract's hash (in both base64 and hex formats)
- The contract's time-to-live (TTL), indicating when it will expire

## Querying Smart Contract Data Entries

Contract data entries store the state data of deployed smart contract instances:

```csharp
private static async Task GetLedgerEntryContractData(string contractId)
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
    Console.WriteLine($"Contract data ledger sequence: {entry.LastModifiedLedgerSeq}");
    Console.WriteLine($"Contract data TTL: {entry.LiveUntilLedger}");
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

This returns information about the contract instance data, including:
- The ledger sequence when it was last modified
- The time-to-live (TTL) for the contract instance

## Querying Trustline Entries

Trustlines represent relationships between accounts and assets they trust. Here's how to query trustline entries:

```csharp
private static async Task GetLedgerEntryTrustline(string accountId, string assetCode, string issuerAccountId)
{
    SorobanServer server = new(TestNetSorobanUrl);

    // Create a non-native asset object
    var asset = Asset.CreateNonNativeAsset(assetCode, issuerAccountId);
    
    // Create a trustline ledger key
    var ledgerKeyTrustline = new LedgerKeyTrustLine(accountId, asset);
    
    // Query the ledger entry
    var trustlineResponse = await server.GetLedgerEntries([ledgerKeyTrustline]);
    var ledgerEntries = trustlineResponse.LedgerEntries;
    
    if (ledgerEntries == null || ledgerEntries.Length == 0)
    {
        Console.WriteLine($"Failed to get trustline entry for account {accountId} and asset {assetCode}");
        return;
    }
    
    var entryTrustline = (LedgerEntryTrustLine)ledgerEntries[0];
    Console.WriteLine($"Account ID: {entryTrustline.AccountID.AccountId}");
    var asset = (AssetTypeCreditAlphaNum4)((TrustlineAsset.Wrapper)ledgerEntry.Asset).Asset;
    Console.WriteLine($"Asset: {asset.Code}:{asset.Issuer}");
    Console.WriteLine($"Balance: {entryTrustline.Balance}");
    Console.WriteLine($"Limit: {entryTrustline.Limit}");
    Console.WriteLine($"Flags: {entryTrustline.Flags}");
    var trustLineExtensionV1 = entryTrustline.TrustlineExtensionV1;
    // Check for trustline extension v1 data if available
    if (trustLineExtensionV1 != null)
    {
        Console.WriteLine($"Buying liabilities: {trustLineExtensionV1.Liabilities.Buying}");
        Console.WriteLine($"Selling liabilities: {trustLineExtensionV1.Liabilities.Selling}");
    }
    var trustLineExtensionV2 = trustLineExtensionV1.TrustlineExtensionV2;
    // Check for trustline extension v2 data if available
    if (trustLineExtensionV2 != null)
    {
        Console.WriteLine($"LiquidityPoolUseCount: {trustLineExtensionV2.LiquidityPoolUseCount}");
    }
}
```

This code retrieves trustline information including:
- Account ID and asset details
- Current balance of the asset
- Trust limit (maximum amount the account can hold)
- Flags indicating special properties (e.g., authorized status)
- Liabilities (if extension data is available)

## Querying Offer Entries

Offers represent orders on Stellar's decentralized exchange. Here's how to query offer entries:

```csharp
private static async Task GetLedgerEntryOffer(string sellerAccountId, string offerId)
{
    SorobanServer server = new(TestNetSorobanUrl);

    
    // Create an offer ledger key
    var ledgerKeyOffer = new LedgerKeyOffer(sellerAccountId, offerId);
    
    // Query the ledger entry
    var offerResponse = await server.GetLedgerEntries([ledgerKeyOffer]);
    var ledgerEntries = offerResponse.LedgerEntries;
    
    if (ledgerEntries == null || ledgerEntries.Length == 0)
    {
        Console.WriteLine($"Failed to get offer entry for offer ID {offerId}");
        return;
    }
    
    var entryOffer = (LedgerEntryOffer)ledgerEntries[0];
    Console.WriteLine($"Offer ID: {entryOffer.OfferId}");
    Console.WriteLine($"Seller: {entryOffer.SellerId.AccountId}");
    Console.WriteLine($"Selling asset: {entryOffer.Selling.CanonicalName()}");
    Console.WriteLine($"Buying asset: {entryOffer.Buying.CanonicalName()}");
    Console.WriteLine($"Amount: {entryOffer.Amount}");
    Console.WriteLine($"Price: {entryOffer.Price.Numerator}/{entryOffer.Price.Denominator}");
    Console.WriteLine($"Flags: {entryOffer.Flags}");
}
```

This retrieves offer information including:
- Offer ID and seller account
- Selling and buying assets
- Amount and price
- Offer flags

## Batch Querying Multiple Ledger Entries

You can also query multiple ledger entries of different types in a single request:

```csharp
private static async Task GetMultipleLedgerEntries(string accountId, string contractId, string balanceId)
{
    SorobanServer server = new(TestNetSorobanUrl);

    // Create multiple ledger keys of different types
    var ledgerKeys = new LedgerKey[]
    {
        new LedgerKeyAccount(accountId),
        CreateLedgerKeyContractData(contractId),
        new LedgerKeyClaimableBalance(balanceId)
    };
    
    // Query all ledger entries in a single request
    var response = await server.GetLedgerEntries(ledgerKeys);
    var ledgerEntries = response.LedgerEntries;
    
    if (ledgerEntries == null || ledgerEntries.Length == 0)
    {
        Console.WriteLine("Failed to get any ledger entries");
        return;
    }
    
    Console.WriteLine($"Retrieved {ledgerEntries.Length} ledger entries");
    
    // Process each entry based on its type
    foreach (var entry in ledgerEntries)
    {
        Console.WriteLine($"Entry type: {entry.GetType().Name}");
        
        if (entry is LedgerEntryAccount accountEntry)
        {
            Console.WriteLine($"Account ID: {accountEntry.Account.AccountId}");
            Console.WriteLine($"Balance: {accountEntry.Balance}");
        }
        else if (entry is LedgerEntryContractData contractDataEntry)
        {
            Console.WriteLine($"Contract data TTL: {contractDataEntry.LiveUntilLedger}");
        }
        else if (entry is LedgerEntryClaimableBalance claimableBalanceEntry)
        {
            Console.WriteLine($"Claimable balance amount: {claimableBalanceEntry.Amount}");
        }
    }
}
```

This powerful approach allows you to efficiently retrieve multiple ledger entries in a single network request.

## Additional Resources

- [Stellar Ledger Documentation](https://developers.stellar.org/docs/learn/fundamentals/stellar-data-structures/ledgers)
- [Soroban RPC API Reference](https://developers.stellar.org/docs/data/apis/rpc/api-reference/methods)
