using StellarDotnetSdk.Examples.Soroban.Helpers;

namespace StellarDotnetSdk.Examples.Soroban.Examples;

/// <summary>
///     Demonstrates how to retrieve the latest ledger information from a Stellar RPC server.
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
        Console.WriteLine($"Latest ledger hash: {response.Id}");
        Console.WriteLine($"Latest ledger close time (unix): {response.CloseTime}");
        Console.WriteLine($"Latest ledger header XDR: {response.HeaderXdr}");
        Console.WriteLine($"Latest ledger metadata XDR length: {response.MetadataXdr?.Length}");
    }
}