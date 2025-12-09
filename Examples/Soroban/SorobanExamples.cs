using System.Net.Sockets;
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
using SCString = StellarDotnetSdk.Soroban.SCString;
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
    private static readonly string TokenWasmPath = Path.GetFullPath("wasm/token_contract.wasm");
    private static readonly string AtomicSwapWasmPath = Path.GetFullPath("wasm/atomic_swap_contract.wasm");
    private static readonly string EventsWasmPath = Path.GetFullPath("wasm/events_contract.wasm");
    private static readonly string IncrementWasmPath = Path.GetFullPath("wasm/increment_contract.wasm");

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
        var wasmId = await UploadContract(keyPair, HelloWasmPath);

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

        Console.WriteLine("\n=== Advanced Real-World Examples ===");

        Console.WriteLine("\n1. Token Contract Example - Deploy and interact with a CAP-46-6 token");
        await TokenContractExample(keyPair, childKeyPair);

        Console.WriteLine("\n2. Increment Contract Example - Persistent storage demonstration");
        await IncrementContractExample(keyPair);

        Console.WriteLine("\n3. Events Contract Example - Publishing and reading contract events");
        await EventsContractExample(keyPair);

        Console.WriteLine("\n4. Atomic Swap Example - Trustless token exchange between parties");
        await AtomicSwapExample(keyPair, childKeyPair, issuerKeyPair);
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

    private static async Task<string> CreateContract(IAccountId keyPair, string wasmId, SCVal[]? args = null)
    {
        SorobanServer server = new(TestNetSorobanUrl);
        // Load the account with the updated sequence number from Soroban server
        var account = await server.GetAccount(keyPair.AccountId);

        var operation = CreateContractOperation.FromAddress(wasmId, account.AccountId, args);

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

    private static async Task<string> UploadContract(IAccountId sourceKeyPair, string wasmPath)
    {
        SorobanServer server = new(TestNetSorobanUrl);
        var wasm = await File.ReadAllBytesAsync(wasmPath);

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

        Console.WriteLine("Sending 'Upload contract' transaction");
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

    #region Advanced Real-World Contract Examples

    /// <summary>
    ///     Demonstrates a complete token contract lifecycle:
    ///     - Deploy a CAP-46-6 compliant token contract with constructor arguments
    ///     - Initialize token metadata (name, symbol, decimals)
    ///     - Mint tokens to an account
    ///     - Transfer tokens between accounts
    ///     - Check balances
    ///     - Approve spending allowances
    ///     - Burn tokens
    ///     Note: This token contract uses a two-phase initialization:
    ///     1. Constructor (called during deployment): Sets the admin
    ///     2. Initialize function (called after deployment): Sets token metadata
    /// </summary>
    private static async Task TokenContractExample(IAccountId admin, IAccountId recipient)
    {
        SorobanServer server = new(TestNetSorobanUrl);

        Console.WriteLine("--- Upload and Deploy Token Contract ---");

        Console.WriteLine("Uploading token contract");
        var tokenWasmId = await UploadContract(admin, TokenWasmPath);

        Console.WriteLine("\n--- Deploy Token Contract with Constructor ---");

        var constructorArgs = new SCVal[]
        {
            new ScAccountId(admin.AccountId),
            new SCUint32(10),
            new SCString("test name"),
            new SCString("a symbol"),
        };

        var tokenContractId = await CreateContract(admin, tokenWasmId, constructorArgs);

        Console.WriteLine($"Token contract ID: {tokenContractId}");

        Console.WriteLine("\n--- Step 2: Mint Tokens ---");
        var adminAccount = await server.GetAccount(admin.AccountId);

        var mintAmount = 1000000L; // 1,000,000 tokens (with 7 decimals = 0.1 actual tokens)
        var mintArgs = new SCVal[]
        {
            new ScAccountId(recipient.AccountId), // to
            new SCInt128(mintAmount.ToString()), // amount
        };

        var mintOp = new InvokeContractOperation(tokenContractId, "mint", mintArgs, admin);
        var mintTx = new TransactionBuilder(adminAccount).AddOperation(mintOp).Build();
        await SimulateAndUpdateTransaction(mintTx, admin);

        var mintResponse = await server.SendTransaction(mintTx);
        ArgumentNullException.ThrowIfNull(mintResponse.Hash);
        await PollTransaction(mintResponse.Hash);
        Console.WriteLine($"Minted {mintAmount} tokens to {recipient.AccountId}");

        Console.WriteLine("\n--- Step 3: Check Balance ---");
        adminAccount = await server.GetAccount(admin.AccountId);

        var balanceArgs = new SCVal[]
        {
            new ScAccountId(recipient.AccountId),
        };

        var balanceOp = new InvokeContractOperation(tokenContractId, "balance", balanceArgs, admin);
        var balanceTx = new TransactionBuilder(adminAccount).AddOperation(balanceOp).Build();
        var balanceSimulation = await server.SimulateTransaction(balanceTx);

        if (balanceSimulation.Results != null && balanceSimulation.Results.Length > 0)
        {
            var balanceXdr = balanceSimulation.Results[0].Xdr;
            ArgumentNullException.ThrowIfNull(balanceXdr);
            var balanceVal = (SCInt128)SCVal.FromXdrBase64(balanceXdr);
            Console.WriteLine($"Balance of {recipient.AccountId}: {balanceVal.Lo}");
        }

        Console.WriteLine("\n--- Step 4: Transfer Tokens ---");
        // Recipient transfers half their tokens back to admin
        // First we need to fund recipient account 
        await HorizonExamples.FundAccountUsingFriendBot(recipient.AccountId);

        var recipientAccount = await server.GetAccount(recipient.AccountId);

        var transferAmount = mintAmount / 2;
        var transferArgs = new SCVal[]
        {
            new ScAccountId(recipient.AccountId), // from
            new ScAccountId(admin.AccountId), // to
            new SCInt128(transferAmount.ToString()), // amount
        };

        var transferOp = new InvokeContractOperation(tokenContractId, "transfer", transferArgs, recipient);
        var transferTx = new TransactionBuilder(recipientAccount).AddOperation(transferOp).Build();
        await SimulateAndUpdateTransaction(transferTx, recipient);

        Console.WriteLine($"Sending 'Transfer token' transaction {transferTx}");
        var transferResponse = await server.SendTransaction(transferTx);
        ArgumentNullException.ThrowIfNull(transferResponse.Hash);
        await PollTransaction(transferResponse.Hash);
        Console.WriteLine($"Transferred {transferAmount} tokens from recipient to admin");

        Console.WriteLine("\n--- Step 5: Approve Allowance ---");
        recipientAccount = await server.GetAccount(recipient.AccountId);

        var latestLedgerResponse = await server.GetLatestLedger();
        var latestLedger = latestLedgerResponse.Sequence;

        var approveAmount = mintAmount / 10;
        var approveArgs = new SCVal[]
        {
            new ScAccountId(recipient.AccountId), // from
            new ScAccountId(admin.AccountId), // spender
            new SCInt128(approveAmount.ToString()), // amount
            new SCUint32((uint)latestLedger + 120000), // expiration_ledger (1 week from the current ledger)
        };

        var approveOp = new InvokeContractOperation(tokenContractId, "approve", approveArgs, recipient);
        var approveTx = new TransactionBuilder(recipientAccount).AddOperation(approveOp).Build();
        await SimulateAndUpdateTransaction(approveTx, recipient);

        Console.WriteLine($"Sending 'Approve allowance' transaction {transferTx}");
        var approveResponse = await server.SendTransaction(approveTx);
        ArgumentNullException.ThrowIfNull(approveResponse.Hash);
        await PollTransaction(approveResponse.Hash);
        Console.WriteLine($"Approved {approveAmount} tokens for admin to spend from recipient's account");

        Console.WriteLine("\n--- Step 6: Burn Tokens ---");
        adminAccount = await server.GetAccount(admin.AccountId);

        var burnAmount = 50000L;
        var burnArgs = new SCVal[]
        {
            new ScAccountId(admin.AccountId), // from
            new SCInt128(burnAmount.ToString()), // amount
        };

        var burnOp = new InvokeContractOperation(tokenContractId, "burn", burnArgs, admin);
        var burnTx = new TransactionBuilder(adminAccount).AddOperation(burnOp).Build();
        await SimulateAndUpdateTransaction(burnTx, admin);

        var burnResponse = await server.SendTransaction(burnTx);
        ArgumentNullException.ThrowIfNull(burnResponse.Hash);
        await PollTransaction(burnResponse.Hash);
        Console.WriteLine($"Burned {burnAmount} tokens from admin account");

        Console.WriteLine("\n✓ Token contract example completed successfully!");
    }

    /// <summary>
    ///     Demonstrates the increment contract showing persistent storage:
    ///     - Deploy an increment contract
    ///     - Increment a counter multiple times
    ///     - Retrieve the current counter value
    /// </summary>
    private static async Task IncrementContractExample(IAccountId keyPair)
    {
        SorobanServer server = new(TestNetSorobanUrl);

        Console.WriteLine("--- Step 1: Deploy Increment Contract ---");

        Console.WriteLine("Uploading increment contract");
        var wasmId = await UploadContract(keyPair, IncrementWasmPath);

        Console.WriteLine("Creating increment contract instance");
        var contractId = await CreateContract(keyPair, wasmId);

        Console.WriteLine($"Increment contract ID: {contractId}");

        Console.WriteLine("--- Step 2: Increment Counter ---");

        var account = await server.GetAccount(keyPair.AccountId);

        var incOp = new InvokeContractOperation(contractId, "increment", null, keyPair);
        var incTx = new TransactionBuilder(account).AddOperation(incOp).Build();
        await SimulateAndUpdateTransaction(incTx, keyPair);

        Console.WriteLine("Sending 'Increment' transaction");
        var incResponse = await server.SendTransaction(incTx);

        ArgumentNullException.ThrowIfNull(incResponse.Hash);
        var response = await PollTransaction(incResponse.Hash);
        ArgumentNullException.ThrowIfNull(response.ResultValue);
        var incrementedValue = (SCUint32)response.ResultValue!;
        Console.WriteLine($"Incremented value: {incrementedValue.InnerValue}");
    }

    /// <summary>
    ///     Demonstrates contract events:
    ///     - Deploy an events contract
    ///     - Invoke functions that publish events
    ///     - Read and parse the published events from transaction results
    /// </summary>
    private static async Task EventsContractExample(IAccountId keyPair)
    {
        SorobanServer server = new(TestNetSorobanUrl);

        Console.WriteLine("--- Step 1: Deploy Events Contract ---");
        Console.WriteLine("Uploading events contract");
        var wasmId = await UploadContract(keyPair, EventsWasmPath);

        Console.WriteLine("Creating events contract instance");
        var contractId = await CreateContract(keyPair, wasmId);

        Console.WriteLine($"Events contract ID: {contractId}");

        Console.WriteLine("\n--- Step 2: Invoke Contract to Publish Events ---");
        var account = await server.GetAccount(keyPair.AccountId);

        var eventOp = new InvokeContractOperation(contractId, "increment", null, keyPair);
        var eventTx = new TransactionBuilder(account).AddOperation(eventOp).Build();
        await SimulateAndUpdateTransaction(eventTx, keyPair);

        var eventResponse = await server.SendTransaction(eventTx);
        ArgumentNullException.ThrowIfNull(eventResponse.Hash);
        var eventResult = await PollTransaction(eventResponse.Hash);

        Console.WriteLine("\n--- Step 3: Read Published Events ---");
        if (eventResult.ResultMetaXdr != null)
        {
            Console.WriteLine("Transaction produced events (check ResultMetaXdr for details)");
            Console.WriteLine("Events are embedded in the transaction metadata");
            Console.WriteLine($"Number of ContractEventsXdr: {eventResult.Events?.ContractEventsXdr?.Length ?? 0}");
            Console.WriteLine($"Number of DiagnosticEventsXdr: {eventResult.Events?.DiagnosticEventsXdr?.Length ?? 0}");
            Console.WriteLine(
                $"Number of TransactionEventsXdr: {eventResult.Events?.TransactionEventsXdr?.Length ?? 0}");
        }

        Console.WriteLine("\nEvents contract example completed successfully!");
    }

    /// <summary>
    ///     Demonstrates atomic swap between two parties:
    ///     - Deploy atomic swap contract
    ///     - Deploy two token contracts (Token A and Token B)
    ///     - Mint tokens to both parties
    ///     - Execute the atomic swap
    ///     - The swap happens atomically (both succeed or both fail)
    /// </summary>
    private static async Task AtomicSwapExample(IAccountId partyA, IAccountId partyB, IAccountId tokenIssuer)
    {
        SorobanServer server = new(TestNetSorobanUrl);

        Console.WriteLine("--- Step 1: Deploy Token A (Party A will swap this) ---");
        Console.WriteLine("Uploading token A contract");
        var tokenAWasmId = await UploadContract(partyA, TokenWasmPath);

        var tokenAConstructorArgs = new SCVal[]
        {
            new ScAccountId(partyA.AccountId), // admin
            new SCUint32(7), // decimals
            new SCString("Token A"), // name
            new SCString("TKNA"), // symbol
        };

        Console.WriteLine("Creating token A contract instance");
        var tokenAContractId = await CreateContract(partyA, tokenAWasmId, tokenAConstructorArgs);
        Console.WriteLine($"Token A contract ID: {tokenAContractId}");

        Console.WriteLine("\n--- Step 2: Deploy Token B (Party B will swap this) ---");
        Console.WriteLine("Uploading token B contract");
        var tokenBWasmId = await UploadContract(partyB, TokenWasmPath);

        var tokenBConstructorArgs = new SCVal[]
        {
            new ScAccountId(partyB.AccountId), // admin
            new SCUint32(7), // decimals
            new SCString("Token B"), // name
            new SCString("TKNB"), // symbol
        };

        Console.WriteLine("Creating token B contract instance");
        var tokenBContractId = await CreateContract(partyB, tokenBWasmId, tokenBConstructorArgs);
        Console.WriteLine($"Token B contract ID: {tokenBContractId}");

        Console.WriteLine("\n--- Step 3: Mint Token A to Party A ---");
        var partyAAccount = await server.GetAccount(partyA.AccountId);
        var amountA = 1000000L; // 0.1 tokens with 7 decimals

        var mintAArgs = new SCVal[]
        {
            new ScAccountId(partyA.AccountId), // to
            new SCInt128(amountA.ToString()), // amount
        };

        var mintAOp = new InvokeContractOperation(tokenAContractId, "mint", mintAArgs, partyA);
        var mintATx = new TransactionBuilder(partyAAccount).AddOperation(mintAOp).Build();
        await SimulateAndUpdateTransaction(mintATx, partyA);

        var mintAResponse = await server.SendTransaction(mintATx);
        ArgumentNullException.ThrowIfNull(mintAResponse.Hash);
        await PollTransaction(mintAResponse.Hash);
        Console.WriteLine($"Minted {amountA} of Token A to Party A");

        Console.WriteLine("\n--- Step 4: Mint Token B to Party B ---");
        var partyBAccount = await server.GetAccount(partyB.AccountId);
        var amountB = 2000000L; // 0.2 tokens with 7 decimals

        var mintBArgs = new SCVal[]
        {
            new ScAccountId(partyB.AccountId), // to
            new SCInt128(amountB.ToString()), // amount
        };

        var mintBOp = new InvokeContractOperation(tokenBContractId, "mint", mintBArgs, partyB);
        var mintBTx = new TransactionBuilder(partyBAccount).AddOperation(mintBOp).Build();
        await SimulateAndUpdateTransaction(mintBTx, partyB);

        var mintBResponse = await server.SendTransaction(mintBTx);
        ArgumentNullException.ThrowIfNull(mintBResponse.Hash);
        await PollTransaction(mintBResponse.Hash);
        Console.WriteLine($"Minted {amountB} of Token B to Party B");

        Console.WriteLine("\n--- Step 5: Deploy Atomic Swap Contract ---");
        Console.WriteLine("Uploading atomic swap contract");
        var swapWasmId = await UploadContract(partyA, AtomicSwapWasmPath);

        Console.WriteLine("Creating atomic swap contract instance");
        var swapContractId = await CreateContract(partyA, swapWasmId);
        Console.WriteLine($"Atomic swap contract ID: {swapContractId}");

        Console.WriteLine("\n--- Step 6: Party A Approves Swap Contract to Spend Token A ---");
        partyAAccount = await server.GetAccount(partyA.AccountId);
        var latestLedger = (await server.GetLatestLedger()).Sequence;

        var approveAArgs = new SCVal[]
        {
            new ScAccountId(partyA.AccountId), // from
            new ScContractId(swapContractId), // spender (swap contract)
            new SCInt128(amountA.ToString()), // amount
            new SCUint32((uint)latestLedger + 200000), // expiration_ledger
        };

        var approveAOp = new InvokeContractOperation(tokenAContractId, "approve", approveAArgs, partyA);
        var approveATx = new TransactionBuilder(partyAAccount).AddOperation(approveAOp).Build();
        await SimulateAndUpdateTransaction(approveATx, partyA);

        var approveAResponse = await server.SendTransaction(approveATx);
        ArgumentNullException.ThrowIfNull(approveAResponse.Hash);
        await PollTransaction(approveAResponse.Hash);
        Console.WriteLine($"Party A approved swap contract to spend {amountA} of Token A");

        Console.WriteLine("\n--- Step 7: Party B Approves Swap Contract to Spend Token B ---");
        partyBAccount = await server.GetAccount(partyB.AccountId);

        var approveBArgs = new SCVal[]
        {
            new ScAccountId(partyB.AccountId), // from
            new ScContractId(swapContractId), // spender (swap contract)
            new SCInt128(amountB.ToString()), // amount
            new SCUint32((uint)latestLedger + 200000), // expiration_ledger
        };

        var approveBOp = new InvokeContractOperation(tokenBContractId, "approve", approveBArgs, partyB);
        var approveBTx = new TransactionBuilder(partyBAccount).AddOperation(approveBOp).Build();
        await SimulateAndUpdateTransaction(approveBTx, partyB);

        var approveBResponse = await server.SendTransaction(approveBTx);
        ArgumentNullException.ThrowIfNull(approveBResponse.Hash);
        await PollTransaction(approveBResponse.Hash);
        Console.WriteLine($"Party B approved swap contract to spend {amountB} of Token B");

        Console.WriteLine("\n--- Step 8: Execute Atomic Swap ---");
        Console.WriteLine("Party A initiates the swap:");
        Console.WriteLine($"  • Party A gives {amountA} of Token A");
        Console.WriteLine($"  • Party A receives {amountB} of Token B");
        Console.WriteLine($"  • Party B gives {amountB} of Token B");
        Console.WriteLine($"  • Party B receives {amountA} of Token A");

        partyAAccount = await server.GetAccount(partyA.AccountId);

        // The swap function typically expects arguments like:
        // swap(a: Address, b: Address, token_a: Address, token_b: Address,
        //      amount_a: i128, min_b_for_a: i128, amount_b: i128, min_a_for_b: i128)
        var swapArgs = new SCVal[]
        {
            new ScAccountId(partyA.AccountId), // party a
            new ScAccountId(partyB.AccountId), // party b
            new ScContractId(tokenAContractId), // token_a
            new ScContractId(tokenBContractId), // token_b
            new SCInt128(amountA.ToString()), // amount_a
            new SCInt128(amountB.ToString()), // min_b_for_a (minimum of token B that A expects)
            new SCInt128(amountB.ToString()), // amount_b
            new SCInt128(amountA.ToString()), // min_a_for_b (minimum of token A that B expects)
        };

        var swapOp = new InvokeContractOperation(swapContractId, "swap", swapArgs, partyA);
        var swapTx = new TransactionBuilder(partyAAccount).AddOperation(swapOp).Build();
        await SimulateAndUpdateTransaction(swapTx, partyA);

        Console.WriteLine("Sending atomic swap transaction...");
        var swapResponse = await server.SendTransaction(swapTx);
        ArgumentNullException.ThrowIfNull(swapResponse.Hash);
        await PollTransaction(swapResponse.Hash);

        Console.WriteLine("\n--- Step 9: Verify Swap Results ---");

        // Check Party A's Token B balance
        partyAAccount = await server.GetAccount(partyA.AccountId);
        var balanceABArgs = new SCVal[] { new ScAccountId(partyA.AccountId) };
        var balanceABOp = new InvokeContractOperation(tokenBContractId, "balance", balanceABArgs, partyA);
        var balanceABTx = new TransactionBuilder(partyAAccount).AddOperation(balanceABOp).Build();
        var balanceABSim = await server.SimulateTransaction(balanceABTx);

        if (balanceABSim.Results != null && balanceABSim.Results.Length > 0)
        {
            var balanceXdr = balanceABSim.Results[0].Xdr;
            ArgumentNullException.ThrowIfNull(balanceXdr);
            var balance = (SCInt128)SCVal.FromXdrBase64(balanceXdr);
            Console.WriteLine($"Party A now has {balance.Lo} of Token B (expected {amountB})");
        }

        // Check Party B's Token A balance
        partyBAccount = await server.GetAccount(partyB.AccountId);
        var balanceBAArgs = new SCVal[] { new ScAccountId(partyB.AccountId) };
        var balanceBAOp = new InvokeContractOperation(tokenAContractId, "balance", balanceBAArgs, partyB);
        var balanceBATx = new TransactionBuilder(partyBAccount).AddOperation(balanceBAOp).Build();
        var balanceBASim = await server.SimulateTransaction(balanceBATx);

        if (balanceBASim.Results != null && balanceBASim.Results.Length > 0)
        {
            var balanceXdr = balanceBASim.Results[0].Xdr;
            ArgumentNullException.ThrowIfNull(balanceXdr);
            var balance = (SCInt128)SCVal.FromXdrBase64(balanceXdr);
            Console.WriteLine($"Party B now has {balance.Lo} of Token A (expected {amountA})");
        }

        Console.WriteLine("\n✓ Atomic swap completed successfully!");
        Console.WriteLine("Key takeaways:");
        Console.WriteLine("  • Both transfers succeeded together (atomicity)");
        Console.WriteLine("  • No intermediary could steal funds");
        Console.WriteLine("  • No trust required between parties");
        Console.WriteLine("  • Smart contract enforced fair exchange");
    }

    #endregion

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
            UseJitter = true,
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
            UseJitter = true,
        };

        // Add additional retriable exceptions for network issues
        resilienceOptions.AdditionalRetriableExceptionTypes.Add(typeof(SocketException));

        // Create client with optional bearer token for authenticated endpoints
        var httpClient = new DefaultStellarSdkHttpClient(
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