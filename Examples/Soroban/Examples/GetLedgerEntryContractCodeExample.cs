using StellarDotnetSdk.Examples.Soroban.Helpers;
using StellarDotnetSdk.LedgerEntries;
using StellarDotnetSdk.LedgerKeys;
using LedgerKey = StellarDotnetSdk.LedgerKeys.LedgerKey;

namespace StellarDotnetSdk.Examples.Soroban.Examples;

/// <summary>
///     Demonstrates how to retrieve contract code ledger entries from the Soroban RPC server.
/// </summary>
internal static class GetLedgerEntryContractCodeExample
{
    public static async Task Run(string contractWasmId)
    {
        Console.WriteLine($"=== Get Ledger Entry Contract Code for {contractWasmId} ===");

        var server = SorobanHelpers.CreateServer();
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
}