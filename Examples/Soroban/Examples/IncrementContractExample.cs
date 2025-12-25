using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Examples.Soroban.Helpers;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Examples.Soroban.Examples;

/// <summary>
///     Demonstrates the increment contract showing persistent storage:
///     - Deploy an increment contract
///     - Increment a counter multiple times
///     - Retrieve the current counter value
/// </summary>
internal static class IncrementContractExample
{
    public static async Task Run(IAccountId keyPair)
    {
        Console.WriteLine("=== Increment Contract Example ===");

        var server = SorobanHelpers.CreateServer();

        Console.WriteLine("\n--- Step 1: Deploy Increment Contract ---");
        var wasmId = await UploadContractExample.Run(keyPair, SorobanWasms.IncrementWasmPath);
        var contractId = await CreateContractExample.Run(keyPair, wasmId);
        Console.WriteLine($"Increment contract ID: {contractId}");

        Console.WriteLine("\n--- Step 2: Increment Counter ---");
        var account = await server.GetAccount(keyPair.AccountId);

        var incOp = new InvokeContractOperation(contractId, "increment", null, keyPair);
        var incTx = new TransactionBuilder(account).AddOperation(incOp).Build();
        await SorobanHelpers.SimulateAndUpdateTransaction(incTx, keyPair);

        Console.WriteLine("Sending 'Increment' transaction");
        var incResponse = await server.SendTransaction(incTx);

        ArgumentNullException.ThrowIfNull(incResponse.Hash);
        var response = await SorobanHelpers.PollTransaction(incResponse.Hash);
        ArgumentNullException.ThrowIfNull(response.ResultValue);

        var incrementedValue = (SCUint32)response.ResultValue;
        Console.WriteLine($"Incremented value: {incrementedValue.InnerValue}");

        Console.WriteLine("\n✓ Increment contract example completed successfully!");
    }
}