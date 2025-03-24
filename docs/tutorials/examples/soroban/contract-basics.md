# Smart Contract Management

This guide demonstrates how to manage smart contracts on the Stellar network using Soroban and the .NET SDK.

## Overview

Soroban is Stellar's smart contract platform that enables developers to build and deploy WebAssembly-based smart contracts. The process generally involves:

1. Uploading a contract (WASM file)
2. Creating a contract instance
3. Invoking contract functions
4. Managing contract lifecycle

## Prerequisites

Before working with Soroban contracts, ensure:

- You have a funded account on the testnet
- You have a compiled WebAssembly contract (.wasm file)
- You've set the appropriate network in your application:
  ```csharp
  Network.UseTestNetwork();
  ```

## Uploading a Contract

The first step is to upload your contract's WebAssembly bytecode to the network:

```csharp
public static async Task<string> UploadContract(IAccountId sourceKeyPair)
{
    // Initialize the Soroban server connection
    SorobanServer server = new(TestNetSorobanUrl);
    
    // Load the WASM bytecode
    var wasm = await File.ReadAllBytesAsync("path/to/your/contract.wasm");

    // Get the account with the latest sequence number
    var account = await server.GetAccount(sourceKeyPair.AccountId);

    // Create the upload operation
    var uploadOperation = new UploadContractOperation(wasm, sourceKeyPair);
    
    // Build the transaction
    var tx = new TransactionBuilder(account)
        .AddOperation(uploadOperation)
        .Build();

    // Simulate the transaction to get the necessary Soroban-specific data
    var simulateResponse = await SimulateAndUpdateTransaction(tx, sourceKeyPair);
    
    // Extract the contract WASM hash from the simulation result
    var result = (SCBytes)SCVal.FromXdrBase64(simulateResponse.Results[0].Xdr);
    
    // Submit the transaction
    var sendResponse = await server.SendTransaction(tx);
    var txHash = sendResponse.Hash;
    
    // Wait for transaction completion
    var getTransactionResponse = await PollTransaction(txHash);
    
    // Return the WASM ID (hash) which will be used to create instances
    var wasmId = getTransactionResponse.WasmHash;
    return wasmId;
}
```

> 💡 **Tip**: The WASM hash (ID) returned by this operation is important - you'll need it to create contract instances.

## Creating a Contract Instance

After uploading the contract, you can create an instance:

```csharp
public static async Task<string> CreateContract(IAccountId keyPair, string wasmId)
{
    SorobanServer server = new(TestNetSorobanUrl);
    
    // Get the account with the latest sequence number
    var account = await server.GetAccount(keyPair.AccountId);

    // Create the contract instance operation
    var operation = CreateContractOperation.FromAddress(wasmId, account.AccountId);

    // Build the transaction
    var tx = new TransactionBuilder(account).AddOperation(operation).Build();
    
    // Simulate and prepare the transaction
    await SimulateAndUpdateTransaction(tx, keyPair);

    // Submit the transaction
    var sendResponse = await server.SendTransaction(tx);
    var txHash = sendResponse.Hash;

    // Wait for transaction completion
    var response = await PollTransaction(txHash);

    // Return the newly created contract ID
    var createdContractId = response.CreatedContractId;
    return createdContractId;
}
```

This creates a contract instance that you can interact with. The contract ID is the unique identifier for this instance.

## Invoking a Contract

Once you have a contract instance, you can call its functions:

```csharp
public static async Task InvokeContract(IAccountId keyPair, string contractId)
{
    SorobanServer server = new(TestNetSorobanUrl);
    
    // Get the account with latest sequence number
    var account = await server.GetAccount(keyPair.AccountId);

    // Create an argument to pass to the contract function
    // In this example, we're calling a "hello" function with a string argument
    var arg = new SCSymbol("gents");
    
    // Create the invoke operation
    var invokeContractOperation = new InvokeContractOperation(
        contractId,  // Contract ID
        "hello",     // Function name
        [arg],       // Arguments array
        keyPair      // Account that will sign
    );
    
    // Build the transaction
    var tx = new TransactionBuilder(account).AddOperation(invokeContractOperation).Build();
    
    // Simulate and prepare the transaction
    await SimulateAndUpdateTransaction(tx, keyPair);

    // Submit the transaction
    var sendResponse = await server.SendTransaction(tx);
    var txHash = sendResponse.Hash;

    // Wait for the transaction to complete
    await PollTransaction(txHash);
}
```

> 📝 **Note**: The arguments you pass must match the types expected by the contract function. The SDK provides various SC* types (SCSymbol, SCBytes, etc.) to represent Soroban contract values.

## Managing Contract Lifecycle (TTL)

Contracts in Soroban have a limited lifetime (TTL - Time To Live). You may need to extend this TTL to keep your contract active:

```csharp
public static async Task ExtendContractFootprintTtl(
    IAccountId keyPair,
    string contractId,
    uint ledgerSequence,
    uint currentTtl)
{
    SorobanServer server = new(TestNetSorobanUrl);
    
    // Get the account with latest sequence number
    var account = await server.GetAccount(keyPair.AccountId);

    // Create an extend operation
    var extendOperation = new ExtendFootprintOperation(currentTtl - ledgerSequence - 1);
    
    // Build the transaction
    var tx = new TransactionBuilder(account).AddOperation(extendOperation).Build();
    
    // Set up the footprint (which ledger entries will be accessed)
    var ledgerFootprint = new LedgerFootprint
    {
        ReadOnly = [CreateLedgerKeyContractData(contractId)],
    };

    // Create resources with the footprint
    var resources = new SorobanResources(ledgerFootprint, 0, 0, 0);
    var transactionData = new SorobanTransactionData(resources, 0);
    tx.SetSorobanTransactionData(transactionData);

    // Simulate and prepare the transaction
    await SimulateAndUpdateTransaction(tx, keyPair);

    // Submit the transaction
    var sendResponse = await server.SendTransaction(tx);
    var txHash = sendResponse.Hash;

    // Wait for transaction completion
    await PollTransaction(txHash);
}
```

Extending the TTL is important for contracts that need to persist data over longer periods.

## Helper Functions

These utility functions are used in the examples above:

### Simulating and Updating a Transaction

```csharp
private static async Task<SimulateTransactionResponse> SimulateAndUpdateTransaction(
    Transaction tx,
    IAccountId signer)
{
    SorobanServer server = new(TestNetSorobanUrl);
    
    // Simulate the transaction to get Soroban-specific details
    var simulateResponse = await server.SimulateTransaction(tx);

    // Update the transaction with Soroban-specific data
    if (simulateResponse.SorobanTransactionData != null)
    {
        tx.SetSorobanTransactionData(simulateResponse.SorobanTransactionData);
    }
    if (simulateResponse.SorobanAuthorization != null)
    {
        tx.SetSorobanAuthorization(simulateResponse.SorobanAuthorization);
    }

    // Add a resource fee
    tx.AddResourceFee((simulateResponse.MinResourceFee ?? 0) + 100000);
    
    // Sign the transaction
    tx.Sign(signer);

    return simulateResponse;
}
```

### Polling for Transaction Completion

```csharp
private static async Task<GetTransactionResponse> PollTransaction(string transactionHash)
{
    SorobanServer server = new(TestNetSorobanUrl);
    var status = TransactionInfo.TransactionStatus.NOT_FOUND;
    GetTransactionResponse? transactionResponse = null;
    
    // Poll until we get a conclusive status
    while (status == TransactionInfo.TransactionStatus.NOT_FOUND)
    {
        await Task.Delay(3000);
        transactionResponse = await server.GetTransaction(transactionHash);

        status = transactionResponse.Status;
        if (status == TransactionInfo.TransactionStatus.FAILED)
        {
            Console.WriteLine($"Transaction {transactionHash} failed");
            break;
        }
        if (status == TransactionInfo.TransactionStatus.SUCCESS)
        {
            Console.WriteLine($"Transaction {transactionHash} was successful");
            return transactionResponse;
        }
        Console.WriteLine("Transaction not updated on-chain. Waiting...");
    }

    return transactionResponse;
}
```

## Additional Resources

- [Soroban Documentation](https://soroban.stellar.org/)
- [WebAssembly Smart Contracts](https://developers.stellar.org/docs/smart-contracts/developing-contracts)
- [Contract Lifecycle Management](https://developers.stellar.org/docs/glossary/time-bounds)
