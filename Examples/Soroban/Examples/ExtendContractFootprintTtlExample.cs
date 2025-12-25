using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Examples.Soroban.Helpers;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Transactions;
using LedgerFootprint = StellarDotnetSdk.Soroban.LedgerFootprint;
using SorobanResources = StellarDotnetSdk.Soroban.SorobanResources;
using SorobanTransactionData = StellarDotnetSdk.Soroban.SorobanTransactionData;

namespace StellarDotnetSdk.Examples.Soroban.Examples;

/// <summary>
///     Demonstrates how to extend the TTL (Time-To-Live) of a contract footprint.
/// </summary>
internal static class ExtendContractFootprintTtlExample
{
    /// <summary>
    ///     Extends the TTL of a contract's footprint by 1.
    /// </summary>
    /// <param name="keyPair">The source account key pair.</param>
    /// <param name="contractId">The contract ID whose TTL to extend.</param>
    /// <param name="ledgerSequence">The current ledger sequence.</param>
    /// <param name="currentTtl">The current TTL of the contract.</param>
    public static async Task Run(
        IAccountId keyPair,
        string contractId,
        uint ledgerSequence,
        uint currentTtl)
    {
        Console.WriteLine($"=== Extend Contract Footprint TTL for {contractId} ===");

        var server = SorobanHelpers.CreateServer();

        // Load the account with the updated sequence number from Soroban server
        var account = await server.GetAccount(keyPair.AccountId);

        var extendOperation = new ExtendFootprintOperation(currentTtl - ledgerSequence - 1);
        var tx = new TransactionBuilder(account).AddOperation(extendOperation).Build();

        var ledgerFootprint = new LedgerFootprint
        {
            ReadOnly = [SorobanHelpers.CreateLedgerKeyContractData(contractId)],
        };

        var resources = new SorobanResources(ledgerFootprint, 0, 0, 0);
        var transactionData = new SorobanTransactionData(resources, 0);
        tx.SetSorobanTransactionData(transactionData);

        await SorobanHelpers.SimulateAndUpdateTransaction(tx, keyPair);

        Console.WriteLine($"Sending 'Extend footprint TTL' transaction with xdr: {tx.ToEnvelopeXdrBase64()}");
        var sendResponse = await server.SendTransaction(tx);

        var txHash = sendResponse.Hash;
        Console.WriteLine($"Extend footprint TTL transaction hash: {txHash}");

        if (sendResponse.ErrorResultXdr != null)
        {
            Console.WriteLine(
                $"Sending 'Extend footprint TTL' transaction failed with error: {sendResponse.ErrorResultXdr}");
        }

        ArgumentNullException.ThrowIfNull(txHash);

        await SorobanHelpers.PollTransaction(txHash);
    }
}