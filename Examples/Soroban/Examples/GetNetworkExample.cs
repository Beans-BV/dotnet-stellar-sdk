using StellarDotnetSdk.Examples.Soroban.Helpers;

namespace StellarDotnetSdk.Examples.Soroban.Examples;

/// <summary>
///     Demonstrates how to retrieve network information from a Soroban RPC server.
/// </summary>
internal static class GetNetworkExample
{
    public static async Task Run()
    {
        Console.WriteLine("=== Get Server Network ===");

        var server = SorobanHelpers.CreateServer();
        var response = await server.GetNetwork();

        Console.WriteLine($"Server passphrase: {response.Passphrase}");
        Console.WriteLine($"Server Friend Bot URL: {response.FriendbotUrl}");
    }
}