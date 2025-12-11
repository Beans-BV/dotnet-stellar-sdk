using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Examples.Soroban.Helpers;
using SCBytes = StellarDotnetSdk.Soroban.SCBytes;
using SCVal = StellarDotnetSdk.Soroban.SCVal;

namespace StellarDotnetSdk.Examples.Soroban.Examples;

/// <summary>
///     Demonstrates how to upload a contract WASM to the Stellar network.
/// </summary>
internal static class UploadContractExample
{
    /// <summary>
    ///     Uploads a contract WASM file to the network.
    /// </summary>
    /// <param name="sourceKeyPair">The source account key pair.</param>
    /// <param name="wasmPath">Path to the WASM file.</param>
    /// <returns>The WASM ID (hash) of the uploaded contract.</returns>
    public static async Task<string> Run(IAccountId sourceKeyPair, string wasmPath)
    {
        Console.WriteLine($"=== Upload Contract from {wasmPath} ===");

        var server = SorobanHelpers.CreateServer();
        var wasm = await File.ReadAllBytesAsync(wasmPath);

        // Load the account with the updated sequence number from Soroban server
        var account = await server.GetAccount(sourceKeyPair.AccountId);

        var uploadOperation = new UploadContractOperation(wasm, sourceKeyPair);
        var tx = new TransactionBuilder(account)
            .AddOperation(uploadOperation)
            .Build();

        var simulateResponse = await SorobanHelpers.SimulateAndUpdateTransaction(tx, sourceKeyPair);

        ArgumentNullException.ThrowIfNull(simulateResponse.Results);
        var xdrBase64 = simulateResponse.Results[0].Xdr;
        ArgumentNullException.ThrowIfNull(xdrBase64);

        var result = (SCBytes)SCVal.FromXdrBase64(xdrBase64);
        Console.WriteLine($"Contract WASM hash: {Convert.ToBase64String(result.InnerValue)}");

        Console.WriteLine("Sending 'Upload contract' transaction");
        var sendResponse = await server.SendTransaction(tx);
        var txHash = sendResponse.Hash;
        Console.WriteLine($"Upload contract transaction hash: {txHash}");

        if (sendResponse.ErrorResultXdr != null)
        {
            Console.WriteLine(
                $"Sending 'Upload contract' transaction failed with error: {sendResponse.ErrorResultXdr}");
        }

        ArgumentNullException.ThrowIfNull(txHash);
        var getTransactionResponse = await SorobanHelpers.PollTransaction(txHash);

        var wasmId = getTransactionResponse.WasmHash;
        ArgumentNullException.ThrowIfNull(wasmId);

        Console.WriteLine($"WASM hash/ID: {wasmId}");
        return wasmId;
    }
}

