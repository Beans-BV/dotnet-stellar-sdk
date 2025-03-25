# Building, Submitting, and Managing Transactions

This guide demonstrates how to build, submit, and manage transactions using the Stellar .NET SDK.

## Understanding Stellar Transactions

In Stellar, a transaction is a unit of work that modifies the ledger. Transactions contain one or more operations (like payments, account creation, etc.) and are atomic - either all operations succeed, or none do. Each transaction includes:

- **Source Account**: The account initiating the transaction
- **Sequence Number**: Ensuring transactions are processed in order
- **Operations**: The actual work to be performed
- **Fee**: The cost of processing the transaction
- **Signatures**: Cryptographic proofs authorizing the transaction

## Building a Simple Transaction

The `TransactionBuilder` class simplifies the process of creating transactions:

```csharp
// Create server connection
var server = new Server(TestNetUrl);

// Load source account data with the latest sequence number
var sourceAccountResponse = await server.Accounts.Account(sourceKeypair.AccountId);

// Create an operation (in this case, a payment)
var operation = new PaymentOperation(destinationKeyPair, asset, amount, sourceKeypair);

// Create transaction and add the operation
var transaction = new TransactionBuilder(sourceAccountResponse)
    .AddOperation(operation)
    .Build();
```

This code creates a transaction with a single payment operation. The `TransactionBuilder` automatically sets the appropriate sequence number based on the account information.

## Adding Multiple Operations

Transactions can contain up to 100 operations. Here's how to create a transaction with multiple operations:

```csharp
// Create server connection
var server = new Server(TestNetUrl);

// Load account data
var accountResponse = await server.Accounts.Account(parentAccountKeyPair.AccountId);

// Create multiple operations
var beginSponsoringOperation = new BeginSponsoringFutureReservesOperation(childKeyPair.AccountId);
var createAccountOperation = new CreateAccountOperation(childKeyPair, "0");
var endSponsoringOperation = new EndSponsoringFutureReservesOperation(childKeyPair);

// Create transaction and add all operations
var transaction = new TransactionBuilder(accountResponse)
    .AddOperation(beginSponsoringOperation)
    .AddOperation(createAccountOperation)
    .AddOperation(endSponsoringOperation)
    .Build();
```

This example shows a transaction with three operations that work together to create a sponsored account.

> 📝 **Note**: Operations in a transaction execute in sequence, so order matters when the outcome of one operation affects another.

## Setting Transaction Options

You can customize various transaction parameters:

```csharp
// Create a transaction with custom options
var transaction = new TransactionBuilder(accountResponse)
    .AddOperation(operation)
    .AddTimeBounds(new TimeBounds(minTime, maxTime))   // Set time bounds
    .AddMemo(Memo.Text("Payment for services"))        // Add a memo
    .SetFee(200000)                                    // Set a custom fee
    .Build();
```

Common options include:
- **Time Bounds**: Specify when the transaction is valid
- **Memo**: Add a note to the transaction
- **Fee**: Set a custom transaction fee

## Signing Transactions

Before submission, transactions must be signed by the appropriate private keys:

```csharp
// Sign the transaction with the source account's key pair
transaction.Sign(sourceKeypair);
```

For transactions requiring multiple signatures:

```csharp
// Sign with multiple keys
transaction.Sign(parentAccountKeyPair);
transaction.Sign(childKeyPair);
```

> ⚠️ **Security Warning**: Never share or expose your secret seed. Sign transactions in a secure environment.

## Submitting Transactions

Once built and signed, you can submit transactions to the network:

```csharp
public static async Task<SubmitTransactionResponse?> SubmitTransaction(Transaction transaction)
{
    // Create server
    var server = new Server(TestNetUrl);
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
        Console.WriteLine($"Transaction hash: {response.Hash}");
        return response;
    }
    catch (Exception exception)
    {
        Console.WriteLine("Failed to submit transaction");
        Console.WriteLine("Exception: " + exception.Message);
    }
    return null;
}
```

This method submits the transaction and handles potential errors or failures.

## Error Handling and Transaction Results

Proper error handling is crucial when working with transactions:

```csharp
try {
    var response = await server.SubmitTransaction(transaction);
    
    if (response.IsSuccess)
    {
        // Transaction succeeded
        Console.WriteLine($"Transaction successful! Hash: {response.Hash}");
    }
    else
    {
        // Transaction failed - get detailed error information
        var resultXdr = response.ResultXdr;
        Console.WriteLine($"Transaction failed: {resultXdr}");
        
        // Parse the XDR to get structured error information
        var transactionResult = TransactionResult.FromXdrBase64(resultXdr);
        
        // Handle specific error cases
        if (!transactionResult.IsSuccess)
        {
            // Look at the result code to understand what went wrong
            Console.WriteLine($"Transaction result code: {transactionResult.GetType().Name}");
        }
    }
}
catch (Exception ex)
{
    // Handle exceptions (network issues, etc.)
    Console.WriteLine($"Error: {ex.Message}");
}
```

Common error scenarios include:
- Insufficient funds
- Invalid sequence number
- Missing or invalid signatures
- Failed operations (e.g., sending to a non-existent account)

## Retrieving Transaction Information

After submission, you can retrieve transaction details:

```csharp
private static async Task GetTransaction(string txHash)
{
    var server = new Server(TestNetUrl);
    // Get Transaction details from Horizon testnet
    var transactionResponse = await server.Transactions.Transaction(txHash);

    Console.WriteLine($"Operation count: {transactionResponse.OperationCount}");
    Console.WriteLine($"Transaction envelope xdr: {transactionResponse.EnvelopeXdr}");
}
```

## Retrieving Transaction Operations

You can also get detailed information about the operations in a transaction:

```csharp
private static async Task<List<OperationResponse>> GetTransactionOperations(string txHash)
{
    var server = new Server(TestNetUrl);
    // Get transaction operation details from Horizon testnet
    var operations = await server.Operations.ForTransaction(txHash).Execute();
    Console.WriteLine($"Operation count: {operations.Records.Count}");
    Console.WriteLine($"Link to operations: {operations.Links}");
    // Get other information if needed
    return operations.Records;
}
```

This retrieves all operations contained in a specific transaction, allowing you to analyze what actions were performed.

## Working with Soroban Transactions

For Soroban smart contract interactions, additional steps are needed:

```csharp
private static async Task<SimulateTransactionResponse> SimulateAndUpdateTransaction(
    Transaction tx,
    IAccountId signer)
{
    SorobanServer server = new(TestNetSorobanUrl);
    var simulateResponse = await server.SimulateTransaction(tx);

    if (simulateResponse.SorobanTransactionData != null)
    {
        tx.SetSorobanTransactionData(simulateResponse.SorobanTransactionData);
    }
    if (simulateResponse.SorobanAuthorization != null)
    {
        tx.SetSorobanAuthorization(simulateResponse.SorobanAuthorization);
    }

    tx.AddResourceFee((simulateResponse.MinResourceFee ?? 0) + 100000);
    tx.Sign(signer);

    return simulateResponse;
}
```

This method simulates a Soroban transaction before submission, then updates it with the required resource information.

## Polling for Transaction Completion

For Soroban transactions especially, you may need to poll for completion:

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

This function continuously checks the status of a transaction until it succeeds or fails, with a delay between checks.

## Best Practices for Transaction Management

Here are some recommended practices when working with Stellar transactions:

1. **Always check sequence numbers**: Ensure you're using the latest account information before building transactions.

2. **Set appropriate fees**: During network congestion, higher fees can help ensure your transaction is processed.

3. **Use time bounds**: For security-sensitive operations, set time bounds to limit how long the transaction is valid.

4. **Handle errors gracefully**: Parse transaction results to provide meaningful feedback to users.

5. **Consider transaction sponsorship**: For accounts with low XLM balances, consider using sponsored operations.

6. **Use appropriate memos**: When sending to exchanges or services, always include any required memo.

## Additional Resources

- [Stellar Transactions Overview](https://developers.stellar.org/docs/learn/fundamentals/transactions)
- [List of Operations](https://developers.stellar.org/docs/learn/fundamentals/transactions/list-of-operations)
- [Transaction Result Codes](https://developers.stellar.org/docs/data/apis/horizon/api-reference/errors/result-codes/transactions)
