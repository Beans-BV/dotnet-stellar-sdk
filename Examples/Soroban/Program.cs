using StellarDotnetSdk.Examples.Horizon;
using StellarDotnetSdk.Examples.Soroban.Examples;
using StellarDotnetSdk.Examples.Soroban.Helpers;

namespace StellarDotnetSdk.Examples.Soroban;

/// <summary>
///     Main entry point and orchestrator for all Soroban examples.
///     Demonstrates various Soroban RPC and smart contract functionalities.
/// </summary>
internal static class Program
{
    private static async Task Main(string[] args)
    {
        Network.UseTestNetwork();

        // Demonstrate retry configuration for Soroban
        await SorobanRetryConfigurationExample.Run();

        Console.WriteLine("\nCreate a key pair");
        var keyPair = HorizonExamples.CreateKeyPair();

        await HorizonExamples.FundAccountUsingFriendBot(keyPair.AccountId);

        Console.WriteLine("\nCreate a child account");
        var (childKeyPair, _) = await HorizonExamples.CreateChildAccountWithSponsorship(keyPair);

        await HorizonExamples.SendNativeAssets(keyPair, childKeyPair.AccountId);

        Console.WriteLine("\nCreate an issuer account");
        var (issuerKeyPair, _) = await HorizonExamples.CreateChildAccountWithSponsorship(keyPair);

        // Basic Soroban RPC Examples
        await RunBasicRpcExamples(keyPair, childKeyPair);

        // Contract Operations Examples
        await RunContractOperationsExamples(keyPair);

        // Advanced Real-World Examples
        await RunAdvancedExamples(keyPair, childKeyPair, issuerKeyPair);
    }

    /// <summary>
    ///     Runs basic Soroban RPC examples demonstrating server queries and ledger entries.
    /// </summary>
    private static async Task RunBasicRpcExamples(
        Accounts.IAccountId keyPair,
        Accounts.IAccountId childKeyPair)
    {
        Console.WriteLine("\n=== Basic Soroban RPC Examples ===");

        Console.WriteLine();
        await GetLedgerEntryAccountExample.Run(keyPair.AccountId);

        Console.WriteLine();
        await GetHealthExample.Run();

        Console.WriteLine();
        await GetNetworkExample.Run();

        Console.WriteLine();
        await GetLatestLedgerExample.Run();

        Console.WriteLine("\nCreate claimable balance");
        var balanceId = await HorizonExamples.CreateClaimableBalance(keyPair, childKeyPair);

        Console.WriteLine();
        await GetLedgerEntryClaimableBalanceExample.Run(balanceId);
    }

    /// <summary>
    ///     Runs contract operation examples demonstrating upload, create, invoke, and TTL management.
    /// </summary>
    private static async Task RunContractOperationsExamples(Accounts.IAccountId keyPair)
    {
        Console.WriteLine("\n=== Contract Operations Examples ===");

        Console.WriteLine("\nUpload hello contract");
        var wasmId = await UploadContractExample.Run(keyPair, SorobanWasms.HelloWasmPath);

        Console.WriteLine();
        await GetLedgerEntryContractCodeExample.Run(wasmId);

        Console.WriteLine("\nCreate a new instance of the uploaded hello contract");
        var createdContractId = await CreateContractExample.Run(keyPair, wasmId);

        Console.WriteLine();
        var (ledgerSeq, ttl) = await GetLedgerEntryContractDataExample.Run(createdContractId);

        Console.WriteLine("\nExtend footprint TTL of the created contract instance by 1");
        await ExtendContractFootprintTtlExample.Run(keyPair, createdContractId, ledgerSeq, ttl);

        Console.WriteLine("\nGet updated ledger entry contract data for the created contract instance");
        var (newSeq, newTtl) = await GetLedgerEntryContractDataExample.Run(createdContractId);
        Console.WriteLine($"New ledger sequence: {newSeq}");
        Console.WriteLine($"Updated ttl is equal to (ttl + 1): {newTtl == ttl + 1}");

        Console.WriteLine();
        await InvokeContractExample.Run(keyPair, createdContractId);
    }

    /// <summary>
    ///     Runs advanced real-world examples demonstrating complex contract interactions.
    /// </summary>
    private static async Task RunAdvancedExamples(
        Accounts.IAccountId keyPair,
        Accounts.IAccountId childKeyPair,
        Accounts.IAccountId issuerKeyPair)
    {
        Console.WriteLine("\n=== Advanced Real-World Examples ===");

        Console.WriteLine("\n1. Token Contract Example - Deploy and interact with a CAP-46-6 token");
        await TokenContractExample.Run(keyPair, childKeyPair);

        Console.WriteLine("\n2. Increment Contract Example - Persistent storage demonstration");
        await IncrementContractExample.Run(keyPair);

        Console.WriteLine("\n3. Events Contract Example - Publishing and reading contract events");
        await EventsContractExample.Run(keyPair);

        Console.WriteLine("\n4. Atomic Swap Example - Trustless token exchange between parties");
        await AtomicSwapExample.Run(keyPair, childKeyPair, issuerKeyPair);
    }
}
