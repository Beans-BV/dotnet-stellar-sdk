using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Examples.Soroban.Helpers;
using SCSymbol = StellarDotnetSdk.Soroban.SCSymbol;

namespace StellarDotnetSdk.Examples.Soroban.Examples;

/// <summary>
///     Demonstrates how to invoke a contract function on the Stellar network.
/// </summary>
internal static class InvokeContractExample
{
    /// <summary>
    ///     Invokes the "hello" function on a hello world contract.
    /// </summary>
    /// <param name="keyPair">The source account key pair.</param>
    /// <param name="contractId">The contract ID to invoke.</param>
    public static async Task Run(IAccountId keyPair, string contractId)
    {
        Console.WriteLine($"=== Invoke Contract {contractId} ===");

        var server = SorobanHelpers.CreateServer();

        // Load the account with the updated sequence number from Soroban server
        var account = await server.GetAccount(keyPair.AccountId);

        var arg = new SCSymbol("gents");
        var invokeContractOperation = new InvokeContractOperation(contractId, "hello", [arg], keyPair);
        var tx = new TransactionBuilder(account).AddOperation(invokeContractOperation).Build();

        await SorobanHelpers.SimulateAndUpdateTransaction(tx, keyPair);

        Console.WriteLine($"Sending 'Invoke contract' transaction with xdr: {tx.ToEnvelopeXdrBase64()}");
        var sendResponse = await server.SendTransaction(tx);

        var txHash = sendResponse.Hash;
        Console.WriteLine($"Invoke contract transaction hash: {txHash}");

        if (sendResponse.ErrorResultXdr != null)
        {
            Console.WriteLine(
                $"Sending 'Invoke contract' transaction failed with error: {sendResponse.ErrorResultXdr}");
            return;
        }

        ArgumentNullException.ThrowIfNull(txHash);

        await SorobanHelpers.PollTransaction(txHash);
    }
}

