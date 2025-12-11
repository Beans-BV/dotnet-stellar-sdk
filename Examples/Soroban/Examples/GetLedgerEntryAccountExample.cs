using StellarDotnetSdk.LedgerEntries;
using StellarDotnetSdk.LedgerKeys;
using StellarDotnetSdk.Examples.Soroban.Helpers;

namespace StellarDotnetSdk.Examples.Soroban.Examples;

/// <summary>
///     Demonstrates how to retrieve account ledger entries from the Soroban RPC server.
/// </summary>
internal static class GetLedgerEntryAccountExample
{
    public static async Task Run(string accountId)
    {
        Console.WriteLine($"=== Get Ledger Entry Account for {accountId} ===");

        var server = SorobanHelpers.CreateServer();
        var ledgerKeyAccount = new LedgerKeyAccount(accountId);
        var ledgerEntriesResponse = await server.GetLedgerEntries([ledgerKeyAccount]);
        var ledgerEntries = ledgerEntriesResponse.LedgerEntries;

        if (ledgerEntries == null || ledgerEntries.Length == 0)
        {
            Console.WriteLine($"Failed to get ledger entry for account {accountId}");
            return;
        }

        var entryAccount = (LedgerEntryAccount)ledgerEntries[0];
        Console.WriteLine($"Entry account ID: {entryAccount.Account.AccountId}");
        Console.WriteLine($"Entry account signing key ID: {entryAccount.Account.SigningKey.AccountId}");
        Console.WriteLine($"Entry account balance: {entryAccount.Balance}");

        var v1 = entryAccount.AccountExtensionV1;
        if (v1 == null)
        {
            return;
        }

        Console.WriteLine($"Entry account balance buying liabilities: {v1.Liabilities.Buying}");
        Console.WriteLine($"Entry account balance selling liabilities: {v1.Liabilities.Selling}");

        var v2 = v1.ExtensionV2;
        if (v2 == null)
        {
            return;
        }

        Console.WriteLine($"Entry account number of sponsored: {v2.NumberSponsored}");
        Console.WriteLine($"Entry account number of sponsoring: {v2.NumberSponsoring}");
    }
}

