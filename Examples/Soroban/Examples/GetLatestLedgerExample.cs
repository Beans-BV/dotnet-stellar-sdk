using StellarDotnetSdk.Examples.Soroban.Helpers;

namespace StellarDotnetSdk.Examples.Soroban.Examples;

/// <summary>
///     Demonstrates how to retrieve the latest ledger information from a Soroban RPC server.
/// </summary>
internal static class GetLatestLedgerExample
{
    public static async Task Run()
    {
        Console.WriteLine("=== Get Latest Ledger ===");

        var server = SorobanHelpers.CreateServer();
        var response = await server.GetLatestLedger();

        Console.WriteLine($"Server protocol version: {response.ProtocolVersion}");
        Console.WriteLine($"Server latest ledger: {response.Sequence}");
    }
}