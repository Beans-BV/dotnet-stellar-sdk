using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Examples.Horizon;
using StellarDotnetSdk.LedgerKeys;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Responses.Operations;
using StellarDotnetSdk.Responses.SorobanRpc;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Xdr;
using SCBytes = StellarDotnetSdk.Soroban.SCBytes;
using SCString = StellarDotnetSdk.Soroban.SCString;
using SCSymbol = StellarDotnetSdk.Soroban.SCSymbol;
using SCVal = StellarDotnetSdk.Soroban.SCVal;
using SCVec = StellarDotnetSdk.Soroban.SCVec;
using Transaction = StellarDotnetSdk.Transactions.Transaction;

namespace StellarDotnetSdk.Examples.Soroban;

internal static class SorobanExamples
{
    private const string TestNetSorobanUrl = "https://soroban-testnet.stellar.org";
    private static readonly string HelloWasmPath = Path.GetFullPath("wasm/hello_world_contract.wasm");

    private static async Task Main(string[] args)
    {
        Network.UseTestNetwork();
        
        Console.WriteLine("Creating a key pair");
        var keyPair = HorizonExamples.CreateKeyPair();

        await HorizonExamples.FundAccountUsingFriendBot(keyPair.AccountId);

        var (childKeyPair, _) = await HorizonExamples.CreateAccount(keyPair);

        await HorizonExamples.SendNativeAssets(sourceKeypair: keyPair, destinationAccountId: childKeyPair.AccountId);

        Console.WriteLine("\nGetting server health");
        await GetHealth();

        Console.WriteLine("\nGetting server network");
        await GetNetwork();

        Console.WriteLine("\nGetting latest ledger");
        await GetLatestLedger();

        Console.WriteLine("\nUpload hello contract");
        var contractId = await UploadContract(keyPair);

        Console.WriteLine("\nInvoke hello contract");
        await InvokeContract(keyPair, contractId);
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

    public static async Task GetLatestLedger()
    {
        SorobanServer server = new(TestNetSorobanUrl);
        var response = await server.GetLatestLedger();
        Console.WriteLine($"Server protocol version: {response.ProtocolVersion}");
        Console.WriteLine($"Server latest ledger: {response.Sequence}");
    }

    public static async Task InvokeContract(IAccountId keyPair, string contractId)
    {
        SorobanServer server = new(TestNetSorobanUrl);
        // Load the account with the updated sequence number from Soroban server
        var account = await server.GetAccount(keyPair.AccountId);

        var address = new SCContractId(contractId);
        var arg = new SCSymbol("gents");
        var functionName = new SCSymbol("hello");
        var invokeContractOperation = new InvokeContractOperation(address, functionName, [arg]);
        var tx = new TransactionBuilder(account).AddOperation(invokeContractOperation).Build();
        await SimulateAndUpdateTransaction(tx, keyPair);
        var sendResponse = await server.SendTransaction(tx);
        var txHash = sendResponse.Hash;
        Console.WriteLine($"Invoke contract transaction hash: {txHash}");
        await PollTransaction(txHash);
    }

    public static async Task<string> UploadContract(IAccountId sourceKeyPair)
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

        var sendResponse = await server.SendTransaction(tx);
        var txHash = sendResponse.Hash;

        Console.WriteLine($"Upload contract transaction hash: {txHash}");

        ArgumentNullException.ThrowIfNull(txHash);
        var getTransactionResponse = await PollTransaction(txHash);

        var contractId = getTransactionResponse.CreatedContractId;
        ArgumentNullException.ThrowIfNull(contractId);

        Console.WriteLine($"Contract ID: {contractId}");
        return contractId;
    }

    // Keep querying for the transaction until success or error
    private static async Task<GetTransactionResponse> PollTransaction(string transactionHash)
    {
        SorobanServer server = new(TestNetSorobanUrl);
        var status = TransactionInfo.TransactionStatus.NOT_FOUND;
        GetTransactionResponse? transactionResponse = null;
        while (status == TransactionInfo.TransactionStatus.NOT_FOUND)
        {
            await Task.Delay(3000);
            transactionResponse = await server.GetTransaction(transactionHash);

            status = transactionResponse.Status;
            if (status == TransactionInfo.TransactionStatus.FAILED)
            {
                Console.WriteLine("Failed");
                break;
            }
            if (status == TransactionInfo.TransactionStatus.SUCCESS)
            {
                ArgumentNullException.ThrowIfNull(transactionResponse.ResultXdr);
                Console.WriteLine($"Success! Transaction hash {transactionHash}");
            }
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

        ArgumentNullException.ThrowIfNull(simulateResponse.SorobanTransactionData);

        tx.SetSorobanTransactionData(simulateResponse.SorobanTransactionData);
        if (simulateResponse.SorobanAuthorization != null)
        {
            tx.SetSorobanAuthorization(simulateResponse.SorobanAuthorization);
        }
        ArgumentNullException.ThrowIfNull(simulateResponse.MinResourceFee);

        tx.AddResourceFee(simulateResponse.MinResourceFee.Value + 100000);
        tx.Sign(signer);

        return simulateResponse;
    }
}