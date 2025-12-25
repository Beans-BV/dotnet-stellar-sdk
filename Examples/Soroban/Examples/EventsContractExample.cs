using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Examples.Soroban.Helpers;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Examples.Soroban.Examples;

/// <summary>
///     Demonstrates contract events:
///     - Deploy an events contract
///     - Invoke functions that publish events
///     - Read and parse the published events from transaction results
/// </summary>
internal static class EventsContractExample
{
    public static async Task Run(IAccountId keyPair)
    {
        Console.WriteLine("=== Events Contract Example ===");

        var server = SorobanHelpers.CreateServer();

        Console.WriteLine("\n--- Step 1: Deploy Events Contract ---");
        var wasmId = await UploadContractExample.Run(keyPair, SorobanWasms.EventsWasmPath);
        var contractId = await CreateContractExample.Run(keyPair, wasmId);
        Console.WriteLine($"Events contract ID: {contractId}");

        Console.WriteLine("\n--- Step 2: Invoke Contract to Publish Events ---");
        var account = await server.GetAccount(keyPair.AccountId);

        var eventOp = new InvokeContractOperation(contractId, "increment", null, keyPair);
        var eventTx = new TransactionBuilder(account).AddOperation(eventOp).Build();
        await SorobanHelpers.SimulateAndUpdateTransaction(eventTx, keyPair);

        var eventResponse = await server.SendTransaction(eventTx);
        ArgumentNullException.ThrowIfNull(eventResponse.Hash);
        var eventResult = await SorobanHelpers.PollTransaction(eventResponse.Hash);

        Console.WriteLine("\n--- Step 3: Read Published Events ---");
        if (eventResult.ResultMetaXdr != null)
        {
            Console.WriteLine("Transaction produced events (check ResultMetaXdr for details)");
            Console.WriteLine("Events are embedded in the transaction metadata");
            Console.WriteLine($"Number of ContractEventsXdr: {eventResult.Events?.ContractEventsXdr?.Length ?? 0}");
            Console.WriteLine($"Number of DiagnosticEventsXdr: {eventResult.Events?.DiagnosticEventsXdr?.Length ?? 0}");
            Console.WriteLine(
                $"Number of TransactionEventsXdr: {eventResult.Events?.TransactionEventsXdr?.Length ?? 0}");
        }

        Console.WriteLine("\n✓ Events contract example completed successfully!");
    }
}