using StellarDotnetSdk.Examples.Soroban.Helpers;
using StellarDotnetSdk.LedgerEntries;
using StellarDotnetSdk.LedgerKeys;

namespace StellarDotnetSdk.Examples.Soroban.Examples;

/// <summary>
///     Demonstrates how to retrieve claimable balance ledger entries from the Soroban RPC server.
/// </summary>
internal static class GetLedgerEntryClaimableBalanceExample
{
    public static async Task Run(string balanceId)
    {
        Console.WriteLine($"=== Get Ledger Entry Claimable Balance for {balanceId} ===");

        var server = SorobanHelpers.CreateServer();
        var ledgerKeyClaimableBalance = new LedgerKeyClaimableBalance(balanceId);
        var ledgerEntriesResponse = await server.GetLedgerEntries([ledgerKeyClaimableBalance]);
        var ledgerEntries = ledgerEntriesResponse.LedgerEntries;

        if (ledgerEntries == null || ledgerEntries.Length == 0)
        {
            Console.WriteLine($"Failed to get ledger entry for claimable balance {balanceId}");
            return;
        }

        var entryClaimableBalance = (LedgerEntryClaimableBalance)ledgerEntries[0];
        Console.WriteLine($"ID: {entryClaimableBalance.BalanceId}");
        Console.WriteLine($"Amount: {entryClaimableBalance.Amount}");

        var claimants = entryClaimableBalance.Claimants;
        Console.WriteLine($"Claimant count: {claimants.Length}");

        for (var i = 0; i < claimants.Length; i++)
        {
            Console.WriteLine($"Claimant {i + 1} address: {claimants[i].Destination.AccountId}");
            Console.WriteLine($"Claimant {i + 1} predicate: {claimants[i].Predicate.GetType()}");
        }
    }
}