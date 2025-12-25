using StellarDotnetSdk.Examples.Soroban.Helpers;
using StellarDotnetSdk.LedgerEntries;

namespace StellarDotnetSdk.Examples.Soroban.Examples;

/// <summary>
///     Demonstrates how to retrieve contract data ledger entries from the Soroban RPC server.
/// </summary>
internal static class GetLedgerEntryContractDataExample
{
    /// <summary>
    ///     Retrieves contract data for a given contract ID.
    /// </summary>
    /// <param name="contractId">The contract ID to query.</param>
    /// <returns>A tuple containing the ledger sequence and TTL.</returns>
    public static async Task<(uint LedgerSeq, uint Ttl)> Run(string contractId)
    {
        Console.WriteLine($"=== Get Ledger Entry Contract Data for {contractId} ===");

        var server = SorobanHelpers.CreateServer();
        var ledgerKeyContractData = SorobanHelpers.CreateLedgerKeyContractData(contractId);
        var contractCodeResponse = await server.GetLedgerEntries([ledgerKeyContractData]);
        var ledgerEntries = contractCodeResponse.LedgerEntries;

        if (ledgerEntries == null || ledgerEntries.Length == 0)
        {
            Console.WriteLine($"Failed to get ledger entries for contract ID {contractId}");
            return (0, 0);
        }

        Console.WriteLine($"Contract data count: {ledgerEntries.Length}");

        var entry = (LedgerEntryContractData)ledgerEntries[0];
        var ledgerSeq = entry.LastModifiedLedgerSeq;
        var ttl = entry.LiveUntilLedger;

        ArgumentNullException.ThrowIfNull(ttl);

        Console.WriteLine($"Contract data ledger sequence: {ledgerSeq}");
        Console.WriteLine($"Contract data TTL: {ttl}");

        return (ledgerSeq, ttl.Value);
    }
}