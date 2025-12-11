using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Examples.Soroban.Helpers;
using SCVal = StellarDotnetSdk.Soroban.SCVal;

namespace StellarDotnetSdk.Examples.Soroban.Examples;

/// <summary>
///     Demonstrates how to create a new contract instance from an uploaded WASM.
/// </summary>
internal static class CreateContractExample
{
    /// <summary>
    ///     Creates a new contract instance from an uploaded WASM.
    /// </summary>
    /// <param name="keyPair">The source account key pair.</param>
    /// <param name="wasmId">The WASM ID (hash) of the uploaded contract.</param>
    /// <param name="args">Optional constructor arguments for the contract.</param>
    /// <returns>The created contract ID.</returns>
    public static async Task<string> Run(IAccountId keyPair, string wasmId, SCVal[]? args = null)
    {
        Console.WriteLine($"=== Create Contract from WASM {wasmId} ===");

        var server = SorobanHelpers.CreateServer();

        // Load the account with the updated sequence number from Soroban server
        var account = await server.GetAccount(keyPair.AccountId);

        var operation = CreateContractOperation.FromAddress(wasmId, account.AccountId, args);

        var tx = new TransactionBuilder(account).AddOperation(operation).Build();
        await SorobanHelpers.SimulateAndUpdateTransaction(tx, keyPair);

        Console.WriteLine($"Sending 'Create contract' transaction with xdr: {tx.ToEnvelopeXdrBase64()}");
        var sendResponse = await server.SendTransaction(tx);

        var txHash = sendResponse.Hash;
        Console.WriteLine($"`Create contract` transaction hash: {txHash}");

        if (sendResponse.ErrorResultXdr != null)
        {
            Console.WriteLine(
                $"Sending 'Create contract' transaction failed with error: {sendResponse.ErrorResultXdr}");
        }

        ArgumentNullException.ThrowIfNull(txHash);

        var response = await SorobanHelpers.PollTransaction(txHash);

        var createdContractId = response.CreatedContractId;
        ArgumentNullException.ThrowIfNull(createdContractId);
        Console.WriteLine($"Created contract ID: {createdContractId}");

        return createdContractId;
    }
}

