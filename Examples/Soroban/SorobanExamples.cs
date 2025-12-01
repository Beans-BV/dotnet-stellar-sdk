using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Examples.Horizon;
using StellarDotnetSdk.LedgerEntries;
using StellarDotnetSdk.LedgerKeys;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses.SorobanRpc;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Xdr;
using LedgerFootprint = StellarDotnetSdk.Soroban.LedgerFootprint;
using LedgerKey = StellarDotnetSdk.LedgerKeys.LedgerKey;
using SCBytes = StellarDotnetSdk.Soroban.SCBytes;
using SCSymbol = StellarDotnetSdk.Soroban.SCSymbol;
using SCVal = StellarDotnetSdk.Soroban.SCVal;
using SorobanResources = StellarDotnetSdk.Soroban.SorobanResources;
using SorobanTransactionData = StellarDotnetSdk.Soroban.SorobanTransactionData;
using Transaction = StellarDotnetSdk.Transactions.Transaction;

namespace StellarDotnetSdk.Examples.Soroban;

internal static class SorobanExamples
{
    private const string TestNetSorobanUrl = "https://soroban-testnet.stellar.org";
    private static readonly string HelloWasmPath = Path.GetFullPath("wasm/hello_world_contract.wasm");

    private static async Task Main(string[] args)
    {
        Network.UseTestNetwork();

        // Demonstrate retry configuration for Soroban
        Console.WriteLine("Soroban HTTP Retry Configuration Examples");
        await DemonstrateSorobanRetryConfiguration();

        Console.WriteLine("Create a key pair");
        var keyPair = HorizonExamples.CreateKeyPair();

        await HorizonExamples.FundAccountUsingFriendBot(keyPair.AccountId);

        Console.WriteLine("\nCreate a child account");
        var (childKeyPair, _) = await HorizonExamples.CreateChildAccountWithSponsorship(keyPair);

        await HorizonExamples.SendNativeAssets(keyPair, childKeyPair.AccountId);

        Console.WriteLine("\nCreate an issue account");
        var (issuerKeyPair, _) = await HorizonExamples.CreateChildAccountWithSponsorship(keyPair);

        Console.WriteLine($"\nGet ledger entry account for {keyPair.AccountId}");
        await GetLedgerEntryAccount(keyPair.AccountId);

        Console.WriteLine("\nGet server health");
        await GetHealth();

        Console.WriteLine("\nGet server network");
        await GetNetwork();

        Console.WriteLine("\nGet latest ledger");
        await GetLatestLedger();

        Console.WriteLine("\nCreate claimable balance");
        var balanceId = await HorizonExamples.CreateClaimableBalance(keyPair, childKeyPair);

        Console.WriteLine($"\nGet ledger entry claimable balance for created balance {balanceId}");
        await GetLedgerEntryClaimableBalance(balanceId);

        Console.WriteLine("\nUpload hello contract");
        var wasmId = await UploadContract(keyPair);

        Console.WriteLine("\nGet ledger entry contract code");
        await GetLedgerEntryContractCode(wasmId);

        Console.WriteLine("\nCreate a new instance of the uploaded hello contract");
        var createdContractId = await CreateContract(keyPair, wasmId);

        Console.WriteLine("\nGet ledger entry contract data for the created contract instance");
        var (ledgerSeq, ttl) = await GetLedgerEntryContractData(createdContractId);

        Console.WriteLine("\nExtend footprint TTL of the created contract instance by 1");
        await ExtendContractFootprintTtl(keyPair, createdContractId, ledgerSeq, ttl);
        Console.WriteLine("Get updated ledger entry contract data for the created contract instance");
        var (newSeq, newTtl) = await GetLedgerEntryContractData(createdContractId);
        Console.WriteLine($"New ledger sequence: {newSeq}");
        Console.WriteLine($"Updated ttl is equal to (ttl + 1): {newTtl == ttl + 1}");

        Console.WriteLine($"\nInvoke contract {createdContractId}");
        await InvokeContract(keyPair, createdContractId);
    }

    private static async Task GetHealth()
    {
        SorobanServer server = new(TestNetSorobanUrl);
        var response = await server.GetHealth();
        Console.WriteLine($"Server health: {response.Status}");
    }

    private static async Task GetNetwork()
    {
        SorobanServer server = new(TestNetSorobanUrl);
        var response = await server.GetNetwork();
        Console.WriteLine($"Server passphrase: {response.Passphrase}");
        Console.WriteLine($"Server Friend Bot URL: {response.FriendbotUrl}");
    }

    private static async Task GetLatestLedger()
    {
        SorobanServer server = new(TestNetSorobanUrl);
        var response = await server.GetLatestLedger();
        Console.WriteLine($"Server protocol version: {response.ProtocolVersion}");
        Console.WriteLine($"Server latest ledger: {response.Sequence}");
    }

    private static async Task<string> CreateContract(IAccountId keyPair, string wasmId)
    {
        SorobanServer server = new(TestNetSorobanUrl);
        // Load the account with the updated sequence number from Soroban server
        var account = await server.GetAccount(keyPair.AccountId);

        var operation = CreateContractOperation.FromAddress(wasmId, account.AccountId);

        var tx = new TransactionBuilder(account).AddOperation(operation).Build();
        await SimulateAndUpdateTransaction(tx, keyPair);

        Console.WriteLine($"Sending 'Create contract' transaction with xdr: {tx.ToEnvelopeXdrBase64()}");
        var sendResponse = await server.SendTransaction(tx);

        var txHash = sendResponse.Hash;
        Console.WriteLine($"`Create contract` transaction hash: {txHash}");

        if (sendResponse.ErrorResultXdr != null)
        {
            Console.WriteLine(
                $"Sending 'Create contract' transaction failed with error: {sendResponse.ErrorResultXdr}");
        }
        ArgumentNullException.ThrowIfNull(txHash);

        var response = await PollTransaction(txHash);

        var createdContractId = response.CreatedContractId;
        ArgumentNullException.ThrowIfNull(createdContractId);
        Console.WriteLine($"Created contract ID: {createdContractId}");
        return createdContractId;
    }

    private static async Task ExtendContractFootprintTtl(
        IAccountId keyPair,
        string contractId,
        uint ledgerSequence,
        uint currentTtl)
    {
        SorobanServer server = new(TestNetSorobanUrl);
        // Load the account with the updated sequence number from Soroban server
        var account = await server.GetAccount(keyPair.AccountId);

        var extendOperation = new ExtendFootprintOperation(currentTtl - ledgerSequence - 1);
        var tx = new TransactionBuilder(account).AddOperation(extendOperation).Build();
        var ledgerFootprint = new LedgerFootprint
        {
            ReadOnly = [CreateLedgerKeyContractData(contractId)],
        };

        var resources = new SorobanResources(ledgerFootprint, 0, 0, 0);
        var transactionData = new SorobanTransactionData(resources, 0);
        tx.SetSorobanTransactionData(transactionData);

        await SimulateAndUpdateTransaction(tx, keyPair);

        Console.WriteLine($"Sending 'Extend footprint TTL' transaction with xdr: {tx.ToEnvelopeXdrBase64()}");
        var sendResponse = await server.SendTransaction(tx);

        var txHash = sendResponse.Hash;
        Console.WriteLine($"Extend footprint TTL transaction hash: {txHash}");

        if (sendResponse.ErrorResultXdr != null)
        {
            Console.WriteLine(
                $"Sending 'Extend footprint TTL' transaction failed with error: {sendResponse.ErrorResultXdr}");
        }
        ArgumentNullException.ThrowIfNull(txHash);

        await PollTransaction(txHash);
    }

    private static async Task InvokeContract(IAccountId keyPair, string contractId)
    {
        SorobanServer server = new(TestNetSorobanUrl);
        // Load the account with the updated sequence number from Soroban server
        var account = await server.GetAccount(keyPair.AccountId);

        var arg = new SCSymbol("gents");
        var invokeContractOperation = new InvokeContractOperation(contractId, "hello", [arg], keyPair);
        var tx = new TransactionBuilder(account).AddOperation(invokeContractOperation).Build();
        await SimulateAndUpdateTransaction(tx, keyPair);

        Console.WriteLine($"Sending 'Invoke contract' transaction with xdr: {tx.ToEnvelopeXdrBase64()}");
        var sendResponse = await server.SendTransaction(tx);

        var txHash = sendResponse.Hash;
        Console.WriteLine($"Invoke contract transaction hash: {txHash}");

        if (sendResponse.ErrorResultXdr != null)
        {
            Console.WriteLine(
                $"Sending 'Invoke contract' transaction failed with error: {sendResponse.ErrorResultXdr}");
            return;
        }
        ArgumentNullException.ThrowIfNull(txHash);

        await PollTransaction(txHash);
    }

    private static async Task<(uint ledgerSeq, uint ttl)> GetLedgerEntryContractData(string contractId)
    {
        SorobanServer server = new(TestNetSorobanUrl);

        var ledgerKeyContractData = CreateLedgerKeyContractData(contractId);
        var contractCodeResponse = await server.GetLedgerEntries([ledgerKeyContractData]);
        var ledgerEntries = contractCodeResponse.LedgerEntries;
        if (ledgerEntries == null || ledgerEntries.Length == 0)
        {
            Console.WriteLine($"Failed to get ledger entries for contract ID {contractId}");
        }
        Console.WriteLine($"Contract data count: {ledgerEntries!.Length}");
        var entry = (LedgerEntryContractData)ledgerEntries[0];
        var ledgerSeq = entry.LastModifiedLedgerSeq;
        var ttl = entry.LiveUntilLedger;
        ArgumentNullException.ThrowIfNull(ttl);
        Console.WriteLine($"Contract data ledger sequence: {ledgerSeq}");
        Console.WriteLine($"Contract data TTL: {ttl}");
        return (ledgerSeq, ttl.Value);
    }

    private static async Task GetLedgerEntryContractCode(string contractWasmId)
    {
        SorobanServer server = new(TestNetSorobanUrl);

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

    private static async Task GetLedgerEntryAccount(string accountId)
    {
        SorobanServer server = new(TestNetSorobanUrl);

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

    private static async Task GetLedgerEntryClaimableBalance(string balanceId)
    {
        SorobanServer server = new(TestNetSorobanUrl);

        var ledgerKeyClaimableBalance = new LedgerKeyClaimableBalance(balanceId);
        Console.WriteLine($"Get ledger entry details for claimable balance {balanceId}");
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


    private static async Task<string> UploadContract(IAccountId sourceKeyPair)
    {
        SorobanServer server = new(TestNetSorobanUrl);
        var wasm = await File.ReadAllBytesAsync(HelloWasmPath);

        // Load the account with the updated sequence number from Soroban server
        var account = await server.GetAccount(sourceKeyPair.AccountId);

        var uploadOperation = new UploadContractOperation(wasm, sourceKeyPair);
        var tx = new TransactionBuilder(account)
            .AddOperation(uploadOperation)
            .Build();

        var simulateResponse = await SimulateAndUpdateTransaction(tx, sourceKeyPair);

        ArgumentNullException.ThrowIfNull(simulateResponse.Results);
        var xdrBase64 = simulateResponse.Results[0].Xdr;
        ArgumentNullException.ThrowIfNull(xdrBase64);

        var result = (SCBytes)SCVal.FromXdrBase64(xdrBase64);
        Console.WriteLine($"Contract WASM hash: {Convert.ToBase64String(result.InnerValue)}");

        Console.WriteLine($"Sending 'Upload contract' transaction with xdr: {tx.ToEnvelopeXdrBase64()}");
        var sendResponse = await server.SendTransaction(tx);
        var txHash = sendResponse.Hash;
        Console.WriteLine($"Upload contract transaction hash: {txHash}");

        if (sendResponse.ErrorResultXdr != null)
        {
            Console.WriteLine(
                $"Sending 'Upload contract' transaction failed with error: {sendResponse.ErrorResultXdr}");
        }

        ArgumentNullException.ThrowIfNull(txHash);
        var getTransactionResponse = await PollTransaction(txHash);

        var wasmId = getTransactionResponse.WasmHash;
        ArgumentNullException.ThrowIfNull(wasmId);

        Console.WriteLine($"WASM hash/ID: {wasmId}");
        return wasmId;
    }

    // Keep querying for the transaction using `GetTransaction` endpoint until success or error
    private static async Task<GetTransactionResponse> PollTransaction(string transactionHash)
    {
        SorobanServer server = new(TestNetSorobanUrl);
        var status = TransactionInfo.TransactionStatus.NOT_FOUND;
        GetTransactionResponse? transactionResponse = null;
        while (status == TransactionInfo.TransactionStatus.NOT_FOUND)
        {
            await Task.Delay(3000);
            Console.WriteLine($"Fetching details for transaction {transactionHash}");
            transactionResponse = await server.GetTransaction(transactionHash);

            status = transactionResponse.Status;
            if (status == TransactionInfo.TransactionStatus.FAILED)
            {
                Console.WriteLine($"Transaction {transactionHash} failed");
                break;
            }
            if (status == TransactionInfo.TransactionStatus.SUCCESS)
            {
                ArgumentNullException.ThrowIfNull(transactionResponse.ResultXdr);
                Console.WriteLine($"Transaction {transactionHash} was successful");
                return transactionResponse;
            }
            Console.WriteLine("Transaction not updated on-chain. Waiting for a moment before retrying..");
        }

        ArgumentNullException.ThrowIfNull(transactionResponse);
        return transactionResponse;
    }

    private static async Task<SimulateTransactionResponse> SimulateAndUpdateTransaction(
        Transaction tx,
        IAccountId signer)
    {
        SorobanServer server = new(TestNetSorobanUrl);
        var simulateResponse = await server.SimulateTransaction(tx);

        if (simulateResponse.SorobanTransactionData != null)
        {
            tx.SetSorobanTransactionData(simulateResponse.SorobanTransactionData);
        }
        if (simulateResponse.SorobanAuthorization != null)
        {
            tx.SetSorobanAuthorization(simulateResponse.SorobanAuthorization);
        }

        tx.AddResourceFee((simulateResponse.MinResourceFee ?? 0) + 100000);
        tx.Sign(signer);

        return simulateResponse;
    }

    private static LedgerKey CreateLedgerKeyContractData(string contractId)
    {
        var scContractId = new ScContractId(contractId);

        var contractDataDurability =
            ContractDataDurability.Create(ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT);
        var ledgerKeyContractData = new LedgerKeyContractData(
            scContractId,
            new SCLedgerKeyContractInstance(),
            contractDataDurability
        );
        return ledgerKeyContractData;
    }

    #region Retry Configuration Examples

    /// <summary>
    ///     Demonstrates HTTP retry configuration for SorobanServer.
    /// </summary>
    private static async Task DemonstrateSorobanRetryConfiguration()
    {
        // Example 1: Default retry configuration
        Console.WriteLine("1. Default Soroban retry configuration:");
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
        var server = new SorobanServer(TestNetSorobanUrl);

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
            UseJitter = true
        };

        var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);
        var server = new SorobanServer(TestNetSorobanUrl, httpClient);

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
        // Production configuration with bearer token and robust connection retry
        var resilienceOptions = new HttpResilienceOptions
        {
            MaxRetryCount = 5,
            BaseDelay = TimeSpan.FromMilliseconds(500),
            MaxDelay = TimeSpan.FromMilliseconds(15000),
            UseJitter = true
        };

        // Add additional retriable exceptions for network issues
        resilienceOptions.AdditionalRetriableExceptionTypes.Add(typeof(System.Net.Sockets.SocketException));

        // Create client with optional bearer token for authenticated endpoints
        var httpClient = new DefaultStellarSdkHttpClient(
            bearerToken: null, // Set your API token here if required
            resilienceOptions: resilienceOptions
        );

        var server = new SorobanServer(TestNetSorobanUrl, httpClient);

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

    #endregion
}