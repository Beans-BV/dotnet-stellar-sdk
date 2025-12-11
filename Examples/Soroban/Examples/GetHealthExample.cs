using StellarDotnetSdk.Examples.Soroban.Helpers;

namespace StellarDotnetSdk.Examples.Soroban.Examples;

/// <summary>
///     Demonstrates how to check the health status of a Soroban RPC server.
/// </summary>
internal static class GetHealthExample
{
    public static async Task Run()
    {
        Console.WriteLine("=== Get Server Health ===");

        var server = SorobanHelpers.CreateServer();
        var response = await server.GetHealth();

        Console.WriteLine($"Server health: {response.Status}");
    }
}

