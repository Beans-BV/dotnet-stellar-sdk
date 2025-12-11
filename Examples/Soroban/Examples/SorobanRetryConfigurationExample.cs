using System.Net.Sockets;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Examples.Soroban.Helpers;

namespace StellarDotnetSdk.Examples.Soroban.Examples;

/// <summary>
///     Demonstrates HTTP retry configuration for SorobanServer.
///     Shows various retry strategies for different use cases.
/// </summary>
internal static class SorobanRetryConfigurationExample
{
    public static async Task Run()
    {
        Console.WriteLine("=== Soroban HTTP Retry Configuration Examples ===");

        // Example 1: Default retry configuration
        Console.WriteLine("\n1. Default Soroban retry configuration:");
        await UseSorobanDefaultRetry();

        // Example 2: Custom retry for smart contract operations
        Console.WriteLine("\n2. Custom retry for contract operations:");
        await UseSorobanCustomRetry();

        // Example 3: High-reliability configuration for production
        Console.WriteLine("\n3. Production-ready retry configuration:");
        await UseSorobanProductionRetry();
    }

    /// <summary>
    ///     Uses default settings with SorobanServer.
    /// </summary>
    private static async Task UseSorobanDefaultRetry()
    {
        // Default constructor - no retries enabled
        var server = SorobanHelpers.CreateServer();

        Console.WriteLine("   No retries enabled - requests fail immediately on connection errors");
        Console.WriteLine("   HTTP status codes are never retried automatically");

        try
        {
            var health = await server.GetHealth();
            Console.WriteLine($"   Server status: {health.Status}");

            var network = await server.GetNetwork();
            Console.WriteLine($"   Network passphrase: {network.Passphrase}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   Request failed: {ex.Message}");
        }
    }

    /// <summary>
    ///     Uses custom retry settings optimized for smart contract operations.
    /// </summary>
    private static async Task UseSorobanCustomRetry()
    {
        // Smart contract operations may need more retries for connection failures
        // due to network instability during long-running operations
        var resilienceOptions = new HttpResilienceOptions
        {
            MaxRetryCount = 5,
            BaseDelay = TimeSpan.FromMilliseconds(300),
            MaxDelay = TimeSpan.FromMilliseconds(8000),
            UseJitter = true,
        };

        var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);
        var server = new SorobanServer(SorobanHelpers.TestNetSorobanUrl, httpClient);

        Console.WriteLine("   Using custom retry: 5 retries, 300ms base delay, 8s max");
        Console.WriteLine("   Only connection failures are retried, not HTTP status codes");

        try
        {
            var latestLedger = await server.GetLatestLedger();
            Console.WriteLine($"   Latest ledger: {latestLedger.Sequence}");
            Console.WriteLine($"   Protocol version: {latestLedger.ProtocolVersion}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   Request failed: {ex.Message}");
        }
    }

    /// <summary>
    ///     Production-ready configuration with comprehensive retry settings for connection failures.
    /// </summary>
    private static async Task UseSorobanProductionRetry()
    {
        // Production configuration with robust connection retry
        var resilienceOptions = new HttpResilienceOptions
        {
            MaxRetryCount = 5,
            BaseDelay = TimeSpan.FromMilliseconds(500),
            MaxDelay = TimeSpan.FromMilliseconds(15000),
            UseJitter = true,
        };

        // Add additional retriable exceptions for network issues
        resilienceOptions.AdditionalRetriableExceptionTypes.Add(typeof(SocketException));

        // Create client with optional bearer token for authenticated endpoints
        var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);

        var server = new SorobanServer(SorobanHelpers.TestNetSorobanUrl, httpClient);

        Console.WriteLine("   Production config: 5 retries, 500ms base, 15s max");
        Console.WriteLine("   Socket exceptions are retriable");
        Console.WriteLine("   HTTP status codes are never retried automatically");

        try
        {
            var health = await server.GetHealth();
            Console.WriteLine($"   Server health: {health.Status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   Request failed after all retries: {ex.Message}");
        }
    }
}

